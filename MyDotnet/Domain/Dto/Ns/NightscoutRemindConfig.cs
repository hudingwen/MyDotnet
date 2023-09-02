namespace MyDotnet.Domain.Dto.Ns
{
    /// <summary>
    /// Nightscout定期检测提醒配置
    /// </summary>
    public class NightscoutRemindConfig
    {
        public string pushWechatID { get; set; }
        public string pushCompanyCode { get; set; }
        public string pushTemplateID { get; set; }
        public string pushUserIDs { get; set; }
        public int preDays { get; set; }
        public int afterDays { get; set; }
    }
}
