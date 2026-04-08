namespace MyDotnet.Domain.Dto.Weitai1
{
    public class Weitai1LoginReturnDto
    {
        public Weitai1LoginReturnDtoInfo info { get; set; }
        public Weitai1LoginReturnDtoContent content { get; set; }
        /// <summary>
        /// 提取token
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// 提取token的过期时间
        /// </summary>
        public DateTime tokenExpire { get; set; }
    }
}
