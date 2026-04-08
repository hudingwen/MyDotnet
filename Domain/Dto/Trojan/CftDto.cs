namespace MyDotnet.Domain.Dto.Trojan
{
    /// <summary>
    /// cdn数据源
    /// </summary>
    public class CftDto
    {
        public string syncToken;
        public string createDate;
        public List<CftDetailDto> prefixes = new List<CftDetailDto>();
    }
}
