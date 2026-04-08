namespace MyDotnet.Domain.Dto.System
{
    /// <summary>
    /// 系统认证相关配置
    /// </summary>
    public static class SysAuthInfo
    {
        /// <summary>
        /// 字典key
        /// </summary>
        public static readonly string KEY = "sys_auth_info";
        /// <summary>
        /// 两步验证(2FA)的颁发者名称
        /// </summary>
        public static readonly string auth_issuer = "auth_issuer";
        /// <summary>
        /// 是否开启验证码 1-开启 0-关闭
        /// </summary>
        public static readonly string login_code_enable = "login_code_enable";
        
    }
}
