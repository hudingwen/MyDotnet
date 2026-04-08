namespace MyDotnet.Domain.Dto.Apple
{
    public class ProfilesReturnAddData
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
        public ProfilesReturnAddDataAttributes attributes { get; set; }
        /// <summary>
        /// 关系
        /// </summary>
        public ProfilesReturnAddDataRelationships relationships { get; set; }

        public AppleLinks links { get; set; }

    }
}
