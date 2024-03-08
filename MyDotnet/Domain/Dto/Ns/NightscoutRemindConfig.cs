namespace MyDotnet.Domain.Dto.Ns
{
    /// <summary>
    /// Nightscout定期检测提醒配置
    /// </summary>
    public class NightscoutRemindConfig
    {
        public string pushUserIDs { get; set; }
        public int preDays { get; set; }
        public int afterDays { get; set; }
        public long serverId { get; set; }
        public bool isOnlyRefreshNginx { get; set; }
    }
}
