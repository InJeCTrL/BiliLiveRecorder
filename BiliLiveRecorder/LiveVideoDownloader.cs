using System;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace BiliLiveRecorder
{
    /// <summary>
    /// 直播视频流下载
    /// </summary>
    class LiveVideoDownloader
    {
        /// <summary>
        /// 视频流下载主动结束事件
        /// </summary>
        public event EventHandler DownloadCompleted;
        /// <summary>
        /// 直播视频流链接
        /// </summary>
        private string LiveVideoLink = string.Empty;
        /// <summary>
        /// 开始下载时间
        /// </summary>
        private DateTime StartTime = DateTime.MinValue;
        /// <summary>
        /// 主播昵称
        /// </summary>
        private string UserName = string.Empty;
        /// <summary>
        /// 输出的视频文件名
        /// </summary>
        private string OutFileName = string.Empty;
        /// <summary>
        /// 用于下载直播视频流的webclient
        /// </summary>
        private WebClient webClient;
        /// <summary>
        /// 是否正在下载
        /// </summary>
        public bool IsDownloading { get; private set; }
        /// <summary>
        /// 初始化直播视频流下载器
        /// </summary>
        /// <param name="LiveVideoLink">直播视频流链接</param>
        /// <param name="StartTime">开始下载时间</param>
        /// <param name="UserName">主播昵称</param>
        public LiveVideoDownloader(string LiveVideoLink, DateTime StartTime, string UserName)
        {
            IsDownloading = false;
            SetVideoLink(LiveVideoLink);
            SetStartTime(StartTime);
            SetUserName(UserName);
        }
        /// <summary>
        /// 设置直播视频流链接
        /// </summary>
        /// <param name="LiveVideoLink">直播视频流链接</param>
        public void SetVideoLink(string LiveVideoLink)
        {
            this.LiveVideoLink = LiveVideoLink;
        }
        /// <summary>
        /// 设置开始下载时间
        /// </summary>
        /// <param name="StartTime">开始下载时间</param>
        public void SetStartTime(DateTime StartTime)
        {
            this.StartTime = StartTime;
        }
        /// <summary>
        /// 设置主播昵称
        /// </summary>
        /// <param name="UserName">主播昵称</param>
        public void SetUserName(string UserName)
        {
            this.UserName = UserName;
        }
        /// <summary>
        /// 检查需要的数据是否准备完毕
        /// </summary>
        /// <returns>准备完毕返回true, 否则返回false</returns>
        private bool DataReady()
        {
            if (LiveVideoLink != string.Empty && StartTime != DateTime.MinValue && UserName != string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 实际执行下载
        /// </summary>
        private void DoDownload()
        {
            // 初始化下载直播流的webclient
            webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.1; rv:73.0) Gecko/20100101 Firefox/73.0");
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            OutFileName = StartTime.ToString("yyyy年MM月dd日HH时mm分ss秒") + "_" + UserName;
            webClient.DownloadFileAsync(new Uri(LiveVideoLink), OutFileName + ".flv", FileMode.Append);
        }
        /// <summary>
        /// 直播视频流下载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // 被外部执行Stop停止下载不触发下载完成事件
            // 文件下载自行结束, 触发下载完成事件
            if (IsDownloading)
            {
                webClient.Dispose();
                IsDownloading = false;
                OnDownloadCompleted(null);
            }
        }
        /// <summary>
        /// 开始下载
        /// </summary>
        /// <returns>新建视频文件名</returns>
        public string Start()
        {
            bool Ready = DataReady();
            if (Ready)
            {
                IsDownloading = true;
                DoDownload();
            }
            return OutFileName;
        }
        /// <summary>
        /// 停止下载
        /// </summary>
        public void Stop()
        {
            if (IsDownloading)
            {
                IsDownloading = false;
                webClient.CancelAsync();
                webClient.Dispose();
            }
        }
        /// <summary>
        /// 下载结束触发事件
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDownloadCompleted(EventArgs e)
        {
            EventHandler eventHandler = DownloadCompleted;
            if (eventHandler != null)
            {
                eventHandler(this, e);
            }
        }
    }
}
