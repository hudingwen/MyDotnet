namespace MyDotnet.Domain.Dto.Yapei
{
    public class YapeiLoginReturnInfoDataTicket
    {
        public string token {  get; set; }
        public long expires { get; set; }
        /// <summary>
        /// token过期时间
        /// </summary>
        public DateTime tokenExpireTime { get; set; }
    }
}
