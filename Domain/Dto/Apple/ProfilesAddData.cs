namespace MyDotnet.Domain.Dto.Apple
{
    public class ProfilesAddData
    {

        /// <summary>
        /// 类型
        /// </summary>
        public string type { get; set; } = "profiles";
        /// <summary>
        /// 属性
        /// </summary>
        public ProfilesAddDataAttributes attributes { get; set; } = new ProfilesAddDataAttributes();
        /// <summary>
        /// 关系
        /// </summary>
        public ProfilesAddDataRelationships relationships { get; set; } = new ProfilesAddDataRelationships();

    }
}
