using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BiliLiveRecoder
{
    public delegate void RecvRoomInfo();
    /// <summary>
    /// LiveMonitor.xaml 的交互逻辑
    /// </summary>
    public partial class LiveMonitor : Window
    {
        public event RecvRoomInfo SetRoomInfo;
        /// <summary>
        /// 直播间监视任务数
        /// </summary>
        public static int MonitorNum = 0;
        /// <summary>
        /// 监视线程
        /// </summary>
        private Thread th_Monitor;
        /// <summary>
        /// 下载线程
        /// </summary>
        private Thread th_Download;
        /// <summary>
        /// 直播流链接
        /// </summary>
        private string LiveLink = null;
        /// <summary>
        /// 正在下载标志
        /// </summary>
        private int IsDownloading = 0;
        /// <summary>
        /// 用于下载的文件流
        /// </summary>
        private FileStream fileStream;
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
            }
            else
            {
                this.Status.Content = "下播";
                this.Status.Foreground = Brushes.Red;
            }
        }
        /// <summary>
        /// 监视函数
        /// </summary>
        private void RoomMonitor()
        {
            // 每隔10s获取一次直播间状态, 若直播开始则下载
            while (true)
            {
                liveInfo = GetRoomInfo(userInfo.UID);
                this.Dispatcher.Invoke(SetRoomInfo);
                // 检测到正在直播且未在下载, 执行异步下载
                if (liveInfo.LiveStatus == 1 && IsDownloading == 0)
                {
                    LiveLink = GetDownloadLink();
                    if (LiveLink != null)
                    {
                        th_Download = new Thread(new ThreadStart(DownloadLive));
                        th_Download.Start();
                        IsDownloading = 1;
                    }
                }
                // 下播但仍处于下载状态
                else if (liveInfo.LiveStatus == 0 && IsDownloading == 1)
                {
                    th_Download.Abort();
                    fileStream.Dispose();
                    fileStream.Close();
                    IsDownloading = 0;
                }
                Thread.Sleep(10000);
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
        /// 下载函数
        /// </summary>
        private void DownloadLive()
        {
            WebClient client = new WebClient();
            client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.1; rv:73.0) Gecko/20100101 Firefox/73.0");
            try
            {
                Stream stream = client.OpenRead(LiveLink);
                fileStream = new FileStream(userInfo.Name + "_" + DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒") + ".flv", FileMode.Append);
                byte[] tBytes = new byte[8192];
                while (true)
                {
                    int n_read = stream.Read(tBytes, 0, 8192);
                    if (n_read > 0)
                    {
                        fileStream.Write(tBytes, 0, n_read);
                        fileStream.Flush();
                    }
                }
            }
            catch
            {
                ;
            }
            finally
            {
                try
                {
                    fileStream.Dispose();
                    fileStream.Close();
                }
                catch
                {
                    ;
                }
                IsDownloading = 0;
            }
        }
        /// <summary>
        /// 窗口关闭前结束监视/下载线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                th_Download.Abort();
                fileStream.Dispose();
                fileStream.Close();
            }
            catch
            {
                ;
            }
            th_Monitor.Abort();
            --MonitorNum;
        }
    }
}
