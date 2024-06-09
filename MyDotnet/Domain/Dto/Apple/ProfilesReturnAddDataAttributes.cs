namespace MyDotnet.Domain.Dto.Apple
{
    public class ProfilesReturnAddDataAttributes
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string profileType { get; set; } 
        /// <summary>
        /// 状态
        /// </summary>
        public string profileState { get; set; }
        /// <summary>
        /// 描述文件(base64)
        /// </summary>
        public string profileContent { get; set; }
        /// <summary>
        /// 设备id
        /// </summary>
        public string uuid { get; set; }
        /// <summary>
        /// 平台类型
        /// </summary>
        public string platform { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public string expirationDate { get; set; } 
    }
}
