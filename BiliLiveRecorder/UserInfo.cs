namespace BiliLiveRecorder
{
    /// <summary>
    /// Bilibili用户信息
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 账号是否真实有效
        /// </summary>
        public bool Valid { get; set; }
        /// <summary>
        /// 唯一数字ID
        /// </summary>
        public int UID { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 头像图片地址
        /// </summary>
        public string FaceURL { get; set; }
        /// <summary>
        /// 签名
        /// </summary>
        public string Sign { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 生日
        /// </summary>
        public string Birthday { get; set; }
    }
}
