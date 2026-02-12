namespace MyDotnet.Domain.Dto.Ns
{
    /// <summary>
    /// CDN可用列表
    /// </summary>
    public class CDNInfoDto
    {
        /// <summary>
        /// 外网CDN
        /// </summary>
        public string defaultCDN;
        /// <summary>
        /// 内网默认CDN
        /// </summary>
        public string defaultCDN_Inner;
        public List<NSCDN> CDNList;
    }
}
