using System;
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
        /// 直播间监视任务数
        /// </summary>
        public static int MonitorNum = 0;
        /// <summary>
        /// 监视线程
        /// </summary>
        private Thread th_Monitor;
        /// <summary>
        /// 用于下载直播流和弹幕的webclient
        /// </summary>
        private WebClient client;
        /// <summary>
        /// 开始录制的时间
        /// </summary>
        private DateTime StartTime;
        /// <summary>
        /// 表示是否正在直播
        /// </summary>
        private bool OnAir = false;
        /// <summary>
        /// 表示是否退出监视
        /// </summary>
        private bool Exit = false;
        /// <summary>
        /// 输出文件名
        /// </summary>
        private string OutFileName = null;
        /// <summary>
        /// 直播流链接
        /// </summary>
        private string LiveLink = null;
        /// <summary>
        /// 传递到的用户信息对象
        /// </summary>
        private UserInfo userInfo;
        /// <summary>
        /// 直播间信息对象
        /// </summary>
        private LiveInfo liveInfo;
        /// <summary>
        /// 直播监视窗口
        /// </summary>
        /// <param name="userInfo">用户信息对象</param>
        public LiveMonitor(UserInfo userInfo)
        {
            InitializeComponent();
            ++MonitorNum;
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
            if (liveInfo.LiveStatus == 1)
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
        /// 监视函数
        /// </summary>
        private void RoomMonitor()
        {
            // 每隔3s获取一次直播间状态, 若直播开始则下载
            while (Exit == false)
            {
                liveInfo = GetRoomInfo(userInfo.UID);
                this.Dispatcher.Invoke(SetRoomInfo);
                // 检测到正在直播且未在下载, 执行下载
                if (liveInfo.LiveStatus == 1)
                {
                    LiveLink = GetDownloadLink();
                    if (LiveLink != null)
                    {
                        OnAir = true;
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
            WebRequest request = WebRequest.Create("https://api.live.bilibili.com/room/v1/Room/getRoomInfoOld?mid=" + userInfo.UID.ToString());
            WebResponse response = request.GetResponse();
            Stream s = response.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            // Json字符串
            string str = sr.ReadToEnd();
            sr.Dispose();
            sr.Close();
            s.Dispose();
            s.Close();
            response.Dispose();
            response.Close();
            // 分离的直播间信息
            string[] items = str.Split(new char[] { ',', '{', '}' }, System.StringSplitOptions.RemoveEmptyEntries);
            LiveInfo liveInfo = new LiveInfo
                                {
                                    LiveStatus = int.Parse(items[6].Substring(13)),
                                    RoomTitle = items[8].Substring(8).Trim(new char[] { '"' }),
                                    RoomID = items[11].Substring(9)
                                };
            return liveInfo;
        }
        /// <summary>
        /// 获得直播流下载链接
        /// </summary>
        /// <returns>成功获取返回链接, 失败返回null</returns>
        private string GetDownloadLink()
        {
            try
            {
                WebRequest request = WebRequest.Create("https://api.live.bilibili.com/room/v1/Room/playUrl?cid=" + liveInfo.RoomID);
                WebResponse response = request.GetResponse();
                Stream s = response.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                // Json字符串
                string str = sr.ReadToEnd();
                sr.Dispose();
                sr.Close();
                s.Dispose();
                s.Close();
                response.Dispose();
                response.Close();
                // 通过特征字符串查找到的直播流地址
                str = str.Substring(str.IndexOf("\"durl\":[{\"url\":\"") + 16).Split(new char[] { '"' })[0].Replace("\\u0026", "&");
                return str;
            }
            catch
            {
                return null;
            }
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
                    while (OnAir)
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
        /// 下载函数
        /// </summary>
        private void DownloadLive()
        {
            // 初始化下载直播流的webclient
            client = new WebClient();
            client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.1; rv:73.0) Gecko/20100101 Firefox/73.0");
            client.DownloadFileCompleted += Client_DownloadFileCompleted;
            StartTime = DateTime.Now;
            OutFileName = userInfo.Name + "_" + StartTime.ToString("yyyy年MM月dd日HH时mm分ss秒");
            client.DownloadFileAsync(new Uri(LiveLink), OutFileName + ".flv");
            FileStream danmuStream = new FileStream(OutFileName + ".xml", FileMode.Append);
            danmuStream.Write(Encoding.UTF8.GetBytes(XMLHeader), 0, XMLHeader.Length);
            danmuStream.Dispose();
            danmuStream.Close();
            DanMuDownloader();
        }
        /// <summary>
        /// 直播流下载完成(下播)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            StopDownload();
        }
        /// <summary>
        /// 停止下载直播流与弹幕
        /// </summary>
        private void StopDownload()
        {
            client.CancelAsync();
            client.Dispose();
            OnAir = false;
        }
        /// <summary>
        /// 弹幕下载函数
        /// </summary>
        /// <param name="network">弹幕接口网络流</param>
        private void DanMuDownloader()
        {
            using (TcpClient tcpClient = new TcpClient("broadcastlv.chat.bilibili.com", 2243))
            {
                NetworkStream network = tcpClient.GetStream();
                SendRoomInfo(network, liveInfo.RoomID);
                StartPingPong(network);
                FileStream danmuStream = new FileStream(OutFileName + ".xml", FileMode.Append);
                // 帧头部16字节
                byte[] Header = new byte[16];
                try
                {
                    while (OnAir)
                    {
                        int n_Read = 16;
                        do
                        {
                            n_Read -= network.Read(Header, 16 - n_Read, n_Read);
                        } while (n_Read > 0 && OnAir);
                        if (!OnAir)
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
                        } while (n_Read > 0 && OnAir);
                        if (!OnAir)
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
                            // 开始PK后5s重新分段下载
                            else if (strJSON.StartsWith("{\"cmd\":\"PK_START\"") == true)
                            {
                                StopDownload();
                                Thread.Sleep(5000);
                                break;
                            }
                            // 结束整场PK后15s重新分段下载
                            else if (strJSON.StartsWith("{\"cmd\":\"PK_MIC_END\"") == true)
                            {
                                StopDownload();
                                Thread.Sleep(15000);
                                break;
                            }
                        }
                    }
                }
                catch(Exception)
                {
                    ;
                    // Console.WriteLine(e.StackTrace);
                    // throw e;
                }
                finally
                {
                    danmuStream.Write(Encoding.UTF8.GetBytes(XMLFooter), 0, XMLFooter.Length);
                    danmuStream.Close();
                }
            }
        }
        /// <summary>
        /// 窗口关闭前结束监视线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Exit = true;
            if (OnAir)
            {
                StopDownload();
            }
            th_Monitor.Join();
            --MonitorNum;
            GC.Collect();
        }
    }
}
