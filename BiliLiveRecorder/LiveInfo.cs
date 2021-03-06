﻿namespace BiliLiveRecorder
{
    /// <summary>
    /// 直播间信息
    /// </summary>
    public class LiveInfo
    {
        /// <summary>
        /// 正在直播状态
        /// </summary>
        public bool OnAir{ get; set; }
        /// <summary>
        /// 直播间标题
        /// </summary>
        public string RoomTitle { get; set; }
        /// <summary>
        /// 直播间ID
        /// </summary>
        public string RoomID { get; set; }
        /// <summary>
        /// 直播视频流链接
        /// </summary>
        public string LiveVideoLink { get; set; }
    }
}
