namespace MyDotnet.Domain.Dto.Apple
{
    public class DevicesReturnAddData
    {

        /// <summary>
        /// 主键
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 属性
        /// </summary>
        public DevicesReturnAddDataAttributes attributes { get; set; } 
        /// <summary>
        /// 链接
        /// </summary>
        public AppleLinks links { get; set; }
    }
}
