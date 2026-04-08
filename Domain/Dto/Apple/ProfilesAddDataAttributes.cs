namespace MyDotnet.Domain.Dto.Apple
{
    public class ProfilesAddDataAttributes
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string profileType { get; set; } = "IOS_APP_DEVELOPMENT";
        /// <summary>
        /// 团队id
        /// </summary>
        //public string teamId { get; set; }
    }
}
