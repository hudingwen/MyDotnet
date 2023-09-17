namespace MyDotnet.Domain.Dto.Ns
{
    /// <summary>
    /// CDN可用列表
    /// </summary>
    public class CDNInfoDto
    {
        public string defaultCDN;
        public List<NSCDN> CDNList;
    }
}
