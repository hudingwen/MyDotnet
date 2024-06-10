namespace MyDotnet.Domain.Dto.Apple
{
    public class DevicesReturnAddDataAttributes
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 平台类型
        /// "IOS", "MAC_OS" 
        /// </summary>
        public string platform { get; set; }
        /// <summary>
        /// udid
        /// </summary>
        public string udid {  get; set; }


        /// <summary>
        /// 设备类型
        ///  "APPLE_WATCH", "IPAD", "IPHONE", "IPOD", "APPLE_TV", "MAC" 
        /// </summary>
        public string deviceClass { get; set; }
        public string model { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 已审核时间(单位:小时)
        /// </summary>
        public double processingTime { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>

        public string addedDate { get; set; }
    }
}
