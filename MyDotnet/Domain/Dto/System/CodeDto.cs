namespace MyDotnet.Domain.Dto.System
{
    /// <summary>
    /// 验证码实体
    /// </summary>
    public class CodeDto
    {
        /// <summary>
        /// 验证码图片 - 图片base64
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 验证码key
        /// </summary>
        public string key {  get; set; }
        /// <summary>
        /// 验证码过期时间
        /// </summary>
        public DateTime expireTime { get; set; }
    }
}
