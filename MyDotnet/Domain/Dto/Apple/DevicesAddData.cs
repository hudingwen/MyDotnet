namespace MyDotnet.Domain.Dto.Apple
{
    public class DevicesAddData
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string type { get; set; } = "devices";
        /// <summary>
        /// 属性
        /// </summary>
        public DevicesAddDataAttributes attributes { get; set; }  =new DevicesAddDataAttributes(); 
    }
}
