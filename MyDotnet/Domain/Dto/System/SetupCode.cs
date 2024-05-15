namespace MyDotnet.Domain.Dto.System
{
    /// <summary>
    /// 谷歌验证dto
    /// </summary>
    public class SetupCode
    {
        /// <summary>
        /// 返回的扫码url
        /// </summary>
        public string provisionUrl;
        /// <summary>
        /// 返回的手动输入key
        /// </summary>
        public string encodedSecretKey;
    }
}
