using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLiveRecorder
{
    /// <summary>
    /// 直播间信息
    /// </summary>
    public class LiveInfo
    {
        /// <summary>
        /// 直播状态
        /// </summary>
        public int LiveStatus { get; set; }
        /// <summary>
        /// 直播间标题
        /// </summary>
        public string RoomTitle { get; set; }
        /// <summary>
        /// 直播间ID
        /// </summary>
        public string RoomID { get; set; }
    }
}
