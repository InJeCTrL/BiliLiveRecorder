using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BiliLiveRecorder
{
    public delegate void RecvRoomInfo();
    /// <summary>
    /// LiveMonitor.xaml 的交互逻辑
    /// </summary>
    public partial class LiveMonitor : Window
    {
        public event RecvRoomInfo SetRoomInfo;
        /// <summary>
        /// 弹幕XML文件的头部
        /// </summary>
        private readonly string XMLHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + 
                                            "<i>" +
                                            "<chatserver>chat.bilibili.com</chatserver>" +
                                            "<chatid>0</chatid>" +
                                            "<mission>0</mission>" +
                                            "<maxlimit>0</maxlimit>" +
                                            "<state>0</state>" +
                                            "<real_name>0</real_name>" +
                                            "<source>k-v</source>";
        /// <summary>
        /// 弹幕XML文件的尾部
        /// </summary>
        private readonly string XMLFooter = "</i>";
        /// <summary>
        /// 监视窗口列表
        /// </summary>
        public static List<LiveMonitor> MonitorList = new List<LiveMonitor>();
        /// <summary>
        /// 是否正在录制
        /// </summary>
        private bool IsRecording = false;
        /// <summary>
        /// 是否退出监视
        /// </summary>
        private bool Exit = false;
        /// <summary>
        /// 监视线程
        /// </summary>
        private Thread th_Monitor;
        /// <summary>
        /// 传递到的用户信息对象
        /// </summary>
        private UserInfo userInfo;
        /// <summary>
        /// 直播间信息对象
        /// </summary>
        private LiveInfo liveInfo;
        /// <summary>
        /// 直播视频流下载器
        /// </summary>
        private LiveVideoDownloader videoDownloader;
        /// <summary>
        /// PK对端直播视频流下载器
        /// </summary>
        private LiveVideoDownloader PKDownloader;
        /// <summary>
        /// 用于下载弹幕的网络流
        /// </summary>
        private NetworkStream network;
        /// <summary>
        /// 开始录制时间
        /// </summary>
        private DateTime StartTime;
        /// <summary>
        /// 直播监视窗口
        /// </summary>
        /// <param name="userInfo">用户信息对象</param>
        public LiveMonitor(UserInfo userInfo)
        {
            InitializeComponent();
            MonitorList.Add(this);
            SetRoomInfo += new RecvRoomInfo(LiveMonitor_SetRoomInfo);
            this.userInfo = userInfo;
            this.Title = "正在监视直播间_" + userInfo.Name;
            this.UID.Content = userInfo.UID.ToString();
            this.Name.Content = userInfo.Name;
            this.Level.Content = userInfo.Level.ToString();
            this.Sex.Content = userInfo.Sex;
            this.Birthday.Content = userInfo.Birthday;
            this.Sign.Text = userInfo.Sign;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(userInfo.FaceURL);
            bitmap.EndInit();
            this.Face.Source = bitmap;
            // 实例化并启动监视线程
            th_Monitor = new Thread(new ThreadStart(RoomMonitor));
            th_Monitor.Start();
        }
        /// <summary>
        /// 接收直播间信息后设置界面显示
        /// </summary>
        private void LiveMonitor_SetRoomInfo()
        {
            this.RoomTitle.Content = liveInfo.RoomTitle;
            this.RoomID.Content = liveInfo.RoomID;
            // 正在直播
            if (liveInfo.OnAir)
            {
                this.Status.Content = "正在直播";
                this.Status.Foreground = Brushes.Green;
                this.MonitorStatus.Content = "下载直播流";
            }
            else
            {
                this.Status.Content = "下播";
                this.Status.Foreground = Brushes.Red;
                this.MonitorStatus.Content = "持续刷新监视";
            }
        }
        /// <summary>
        /// 获取get请求的传回数据
        /// </summary>
        /// <param name="URL">链接</param>
        /// <returns>传回页面字符串</returns>
        private string FetchGetResponse(string URL)
        {
            try
            {
                WebRequest request = WebRequest.Create(URL);
                request.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
                WebResponse response = request.GetResponse();
                Stream s = response.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                string str = sr.ReadToEnd();
                sr.Dispose();
                sr.Close();
                s.Dispose();
                s.Close();
                response.Dispose();
                response.Close();
                return str;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 监视函数
        /// </summary>
        private void RoomMonitor()
        {
            // 每隔3s获取一次直播间状态, 若直播开始则下载
            while (!Exit)
            {
                liveInfo = GetRoomInfo(userInfo.UID);
                this.Dispatcher.Invoke(SetRoomInfo);
                // 检测到正在直播执行下载
                if (liveInfo.OnAir)
                {
                    liveInfo.LiveVideoLink = GetDownloadLink(liveInfo.RoomID);
                    if (liveInfo.LiveVideoLink != null)
                    {
                        IsRecording = true;
                        DownloadLive();
                        continue;
                    }
                }
                Thread.Sleep(3000);
            }
        }
        /// <summary>
        /// 获取直播间信息
        /// </summary>
        /// <param name="UID">用户UID</param>
        /// <returns>直播间信息对象</returns>
        private LiveInfo GetRoomInfo(int UID)
        {
            // Json字符串
            string str = FetchGetResponse("https://api.live.bilibili.com/room/v1/Room/getRoomInfoOld?mid=" + userInfo.UID.ToString());
            LiveInfo liveInfo = new LiveInfo
            {
                OnAir = false,
                RoomTitle = "",
                RoomID = ""
            };
            if (str != null)
            {
                // 直播状态
                string liveStatus = str.Substring(str.IndexOf("\"liveStatus\":") + 13, 1);
                if (liveStatus == "1")
                {
                    liveInfo.OnAir = true;
                }
                // 直播间名称
                string RoomTitle = str.Substring(str.IndexOf("\"title\":\"") + 9);
                RoomTitle = RoomTitle.Substring(0, RoomTitle.IndexOf('"'));
                liveInfo.RoomTitle = RoomTitle;
                // 直播间ID
                string RoomID = str.Substring(str.IndexOf("\"roomid\":") + 9);
                RoomID = RoomID.Substring(0, RoomID.IndexOfAny(new char[] { ',', '}' }));
                liveInfo.RoomID = RoomID;
            }
            return liveInfo;
        }
        /// <summary>
        /// 获得直播流下载链接
        /// </summary>
        /// <returns>成功获取返回链接, 失败返回null</returns>
        private string GetDownloadLink(string RoomID)
        {
            string str = FetchGetResponse("https://api.live.bilibili.com/room/v1/Room/playUrl?cid=" + RoomID + "&qn=0&platform=web");
            if (str != null)
            {
                // 通过特征字符串查找到的直播流地址
                str = str.Substring(str.IndexOf("\"durl\":[{\"url\":\"") + 16);
                str = str.Substring(0, str.IndexOf('"')).Replace("\\u0026", "&");
            }
            return str;
        }
        /// <summary>
        /// 发送直播间信息到弹幕服务器
        /// </summary>
        /// <param name="network">弹幕接口网络流</param>
        /// <param name="RoomID">直播间ID</param>
        private void SendRoomInfo(NetworkStream network, string RoomID)
        {
            string Info = "{\"uid\":0,\"roomid\":" + RoomID + "}";
            int PayloadSize = Info.Length + 16;
            byte[] Payload = new byte[PayloadSize];
            MemoryStream ms_Payload = new MemoryStream(Payload);
            ms_Payload.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(PayloadSize)), 0, 4);
            ms_Payload.Write(new byte[] { 0x00, 0x10, 0x00, 0x01, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x01 }, 0, 12);
            ms_Payload.Write(Encoding.UTF8.GetBytes(Info), 0, Info.Length);
            ms_Payload.Dispose();
            ms_Payload.Close();
            network.Write(Payload, 0, PayloadSize);
            network.Flush();
        }
        /// <summary>
        /// 开始向弹幕服务器发送心跳包
        /// </summary>
        /// <param name="network">弹幕接口网络流</param>
        private async void StartPingPong(NetworkStream network)
        {
            await Task.Run(() =>
            {
                try
                {
                    while (IsRecording)
                    {
                        network.WriteAsync(new byte[] { 0x00, 0x00, 0x00, 0x10, 0x00, 0x10, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01 }, 0, 16);
                        network.FlushAsync();
                        Thread.Sleep(30000);
                    }
                }
                catch
                {
                    ;
                }
            });
        }
        /// <summary>
        /// 获取本场PKID
        /// </summary>
        /// <param name="RoomID">房间号</param>
        /// <returns>PKID</returns>
        private string GetPKID(string RoomID)
        {
            string str = FetchGetResponse("https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom?room_id=" + RoomID);
            int i_pkid = str.IndexOf("\"pk_id\":");
            // 不在PK状态
            if (i_pkid == -1)
            {
                return string.Empty;
            }
            str = str.Substring(i_pkid + 8);
            str = str.Substring(0, str.IndexOfAny(new char[] { '}', ',' }));
            return str;
        }
        /// <summary>
        /// 获取PK对端信息
        /// </summary>
        /// <param name="PKID">PK编号</param>
        /// <returns>对端信息</returns>
        private PKInfo GetPKMatch(string PKID)
        {
            string str = FetchGetResponse("https://api.live.bilibili.com/av/v1/Pk/getInfoById?pk_id=" + PKID);
            string FirstUID = str.Substring(str.IndexOf("\"uid\":") + 6);
            FirstUID = FirstUID.Substring(0, FirstUID.IndexOfAny(new char[] { ',', '}' }));
            // 第一个UID不是对端用户, 找第二个UID
            if (FirstUID.Equals(userInfo.UID.ToString()))
            {
                // 第二个UID
                string SecondUID = str.Substring(str.LastIndexOf("\"uid\":") + 6);
                SecondUID = SecondUID.Substring(0, SecondUID.IndexOfAny(new char[] { ',', '}' }));
                // 房间号
                string match_id = str.Substring(str.IndexOf("\"match_id\":") + 11);
                match_id = match_id.Substring(0, match_id.IndexOfAny(new char[] { ',', '}' }));
                // 昵称
                string UName = str.Substring(str.LastIndexOf("\"uname\":\"") + 9);
                UName = UName.Substring(0, UName.IndexOf('"'));
                return new PKInfo
                {
                    UID = int.Parse(SecondUID),
                    RoomID = match_id,
                    Name = UName
                };
            }
            // 第一个UID是对端用户
            else
            {
                // 房间号
                string Init_id = str.Substring(str.IndexOf("\"init_id\":") + 10);
                Init_id = Init_id.Substring(0, Init_id.IndexOfAny(new char[] { ',', '}' }));
                // 昵称
                string UName = str.Substring(str.IndexOf("\"uname\":\"") + 9);
                UName = UName.Substring(0, UName.IndexOf('"'));
                return new PKInfo
                {
                    UID = int.Parse(FirstUID),
                    RoomID = Init_id,
                    Name = UName
                };
            }
        }
        /// <summary>
        /// 尝试录制对端PK画面
        /// </summary>
        private void RecordOtherPK()
        {
            // PK对端仍在录制, 本端重试不重录对端
            if (PKDownloader == null || PKDownloader.IsDownloading == false)
            {
                // 获取PK编号
                string PK_ID = GetPKID(liveInfo.RoomID);
                // 若当前直播间正在PK则对端一并录制
                if (PK_ID != string.Empty)
                {
                    // 获取PK对端数据
                    PKInfo pKInfo = GetPKMatch(PK_ID);
                    string PKLiveURL = GetDownloadLink(pKInfo.RoomID);
                    if (PKLiveURL != null)
                    {
                        PKDownloader = new LiveVideoDownloader(PKLiveURL, StartTime, pKInfo.Name + "【PK双录】");
                        PKDownloader.DownloadCompleted += PKDownloader_DownloadCompleted;
                        PKDownloader.Start();
                    }
                }
            }
        }
        /// <summary>
        /// 下载函数
        /// </summary>
        private void DownloadLive()
        {
            StartTime = DateTime.Now;
            videoDownloader = new LiveVideoDownloader(liveInfo.LiveVideoLink, StartTime, userInfo.Name);
            videoDownloader.DownloadCompleted += VideoDownloader_DownloadCompleted;
            string OutFileName = videoDownloader.Start();
            RecordOtherPK();
            FileStream danmuStream = new FileStream(OutFileName + ".xml", FileMode.Append);
            danmuStream.Write(Encoding.UTF8.GetBytes(XMLHeader), 0, XMLHeader.Length);
            danmuStream.Dispose();
            danmuStream.Close();
            DanMuDownloader(OutFileName);
        }
        /// <summary>
        /// PK对端视频流下载自行结束（非主动调用Stop）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PKDownloader_DownloadCompleted(object sender, EventArgs e)
        {
            if (!Exit)
            {
                RecordOtherPK();
            }
        }
        /// <summary>
        /// 直播视频流下载自行结束（非主动调用Stop）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoDownloader_DownloadCompleted(object sender, EventArgs e)
        {
            try
            {
                IsRecording = false;
                network.Close();
            }
            catch
            {
                ;
            }
        }
        /// <summary>
        /// 等待直播视频流地址切换
        /// </summary>
        private void WaitForSwitch()
        {
            do
            {
                if (!GetDownloadLink(liveInfo.RoomID).Equals(liveInfo.RoomID))
                {
                    break;
                }
            } while (true);
        }
        /// <summary>
        /// 等待进入PK开始状态
        /// </summary>
        private void WaitForPKStart()
        {
            do
            {
                if (!GetPKID(liveInfo.RoomID).Equals(string.Empty))
                {
                    break;
                }
            } while (true);
        }
        /// <summary>
        /// 等待进入PK结束状态
        /// </summary>
        private void WaitForPKComplete()
        {
            do
            {
                if (GetPKID(liveInfo.RoomID).Equals(string.Empty))
                {
                    break;
                }
            } while (true);
        }
        /// <summary>
        /// 弹幕下载函数
        /// </summary>
        /// <param name="OutFileName">输出文件名</param>
        private void DanMuDownloader(string OutFileName)
        {
            using (TcpClient tcpClient = new TcpClient("broadcastlv.chat.bilibili.com", 2243))
            {
                network = tcpClient.GetStream();
                SendRoomInfo(network, liveInfo.RoomID);
                StartPingPong(network);
                FileStream danmuStream = new FileStream(OutFileName + ".xml", FileMode.Append);
                // 帧头部16字节
                byte[] Header = new byte[16];
                try
                {
                    while (IsRecording)
                    {
                        int n_Read = 16;
                        do
                        {
                            n_Read -= network.Read(Header, 16 - n_Read, n_Read);
                        } while (n_Read > 0 && IsRecording);
                        if (!IsRecording)
                        {
                            break;
                        }
                        // JSON字节数
                        int JSONSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(Header, 0)) - 16;
                        // JSON大小小等于0
                        if (JSONSize <= 0)
                        {
                            continue;
                        }
                        // 消息类型
                        int MSG_Type = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(Header, 8));
                        byte[] JSONData = new byte[JSONSize];
                        n_Read = JSONSize;
                        do
                        {
                            n_Read -= network.Read(JSONData, JSONSize - n_Read, n_Read);
                        } while (n_Read > 0 && IsRecording);
                        if (!IsRecording)
                        {
                            break;
                        }
                        // 服务器传来命令
                        if (MSG_Type == 5)
                        {
                            string strJSON = Encoding.UTF8.GetString(JSONData);
                            // 收到弹幕数据
                            if (strJSON.StartsWith("{\"cmd\":\"DANMU_MSG\"") == true)
                            {
                                string Info0 = strJSON.Substring(strJSON.IndexOf("\"info\":[[") + 9);
                                string msg = Info0.Substring(Info0.IndexOf(']') + 3);
                                msg = msg.Substring(0, msg.IndexOf("\",["));
                                Info0 = Info0.Substring(0, Info0.IndexOf(']'));
                                string[] Info0Items = Info0.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                byte[] Data = Encoding.UTF8.GetBytes("<d p=\"" + (DateTime.Now - StartTime).TotalSeconds.ToString() + "," + Info0Items[1] + "," + Info0Items[2] + "," + Info0Items[3] + "," + Info0Items[4] + ",0," + Info0Items[7].Trim(new char[] { '"' }) + ",0\">" + msg + "</d>");
                                danmuStream.Write(Data, 0, Data.Length);
                                danmuStream.Flush();
                            }
                            // 开始PK后5s再重新分段下载
                            else if (strJSON.StartsWith("{\"cmd\":\"PK_START\"") == true)
                            {
                                videoDownloader.Stop();
                                //WaitForSwitch();
                                //WaitForPKStart();
                                network.Close();
                                Thread.Sleep(5000);
                                break;
                            }
                            // 结束整场PK后15s再重新分段下载
                            else if (strJSON.StartsWith("{\"cmd\":\"PK_MIC_END\"") == true)
                            {
                                videoDownloader.Stop();
                                PKDownloader.Stop();
                                //WaitForSwitch();
                                //WaitForPKComplete();
                                network.Close();
                                Thread.Sleep(15000);
                                break;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ;
                }
                finally
                {
                    danmuStream.Write(Encoding.UTF8.GetBytes(XMLFooter), 0, XMLFooter.Length);
                    danmuStream.Close();
                    IsRecording = false;
                }
            }
        }
        /// <summary>
        /// 结束监视、录制
        /// </summary>
        public void CloseMonitor()
        {
            Exit = true;
            if (PKDownloader != null)
            {
                PKDownloader.Stop();
            }
            if (IsRecording)
            {
                videoDownloader.Stop();
                IsRecording = false;
                network.Close();
            }
        }
        /// <summary>
        /// 窗口关闭前结束监视线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseMonitor();
            MonitorList.Remove(this);
        }
    }
}
