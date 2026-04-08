namespace MyDotnet.Domain.Dto.Apple
{
    public class DevicesAddDataAttributes
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 平台类型
        /// "IOS", "MAC_OS" 
        /// </summary>
        public string platform { get; set; } = "IOS";
        /// <summary>
        /// udid
        /// </summary>
        public string udid {  get; set; } 
    }
}
