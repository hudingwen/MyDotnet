using MyDotnet.Domain.Entity.Base;

namespace MyDotnet.Domain.Entity.System
{
    /// <summary>
    /// 双因子验证表
    /// </summary>
    public class UserGoogleAuthenticator: BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public long userId { get; set; }
        /// <summary>
        /// 生成的key
        /// </summary>
        public string key { get; set; }
        /// <summary>
        /// 生成的key
        /// </summary>
        public string keyBase32 { get; set; }
        /// <summary>
        /// 生成扫码地址
        /// </summary>
        public string provisionUrl { get; set; }
        /// <summary>
        /// 颁发者
        /// </summary>
        public string issuer { get; set; }
        /// <summary>
        /// 用户
        /// </summary>
        public string user { get; set; }

    }
}
