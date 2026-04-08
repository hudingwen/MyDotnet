namespace MyDotnet.Domain.Dto.Apple
{
    public class ProfilesReturnAddDataRelationshipsDevicesData
    {

        public string type { get; set; }
        public string id { get; set; }



        /// <summary>
        /// 设备信息(查找添加)
        /// </summary>
        public DevicesReturnAddDataAttributes attributes { get; set; }
    }
}
