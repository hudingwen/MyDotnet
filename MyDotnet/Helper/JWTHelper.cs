using MyDotnet.Domain.Dto.System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyDotnet.Helper
{
    public static class JWTHelper
    {
        public static PermissionRequirement permissionRequirement { get; set; }

        /// <summary>
        /// 颁发JWT
        /// </summary>
        /// <param name="tokenModel">当前颁发对象的用户信息</param>
        /// <returns>JWT字符串</returns>
        public static string IssueJwt(JwtUserInfo jwtUserInfo)
        {
            //var permissionRequirement = InternalHelper.RootServices.GetService<PermissionRequirement>();
            //获取JWT配置
            string iss = permissionRequirement.Issuer;//颁发者
            string aud = permissionRequirement.Audience;//使用者 

            //添加用户信息和过期时间
            var claimsIdentity = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, jwtUserInfo.Uid.ToString()), //JWT ID
                new Claim(ClaimTypes.Name,jwtUserInfo.Name.ObjToString()), //JWT ID
                new Claim(JwtRegisteredClaimNames.Iat, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),//JWT的发布时间
                new Claim(JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddSeconds(3600)).ToUnixTimeSeconds()}"),//JWT到期时间
                new Claim(ClaimTypes.Expiration,$"{new DateTimeOffset(DateTime.Now.AddSeconds(3600)).ToUnixTimeSeconds()}"),//JWT到期时间
                new Claim(JwtRegisteredClaimNames.Iss,iss), //颁发者
                new Claim(JwtRegisteredClaimNames.Aud,aud)//使用者
            };


            //添加用户角色
            var claimRoleList = jwtUserInfo.Roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
            claimsIdentity.AddRange(claimRoleList);

            //封装用户信息
            var jwt = new JwtSecurityToken(
              issuer: iss,
              claims: claimsIdentity,
              signingCredentials: permissionRequirement.SigningCredentials);

            //返回token信息
            var jwtHandler = new JwtSecurityTokenHandler();
            return jwtHandler.WriteToken(jwt);
        }

        /// <summary>
        /// 解析JWT字符串
        /// </summary>
        /// <param name="jwtStr">JWT加密的字符</param>
        /// <returns>JWT中的用户信息</returns>
        public static JwtUserInfo SerializeJwtStr(string jwtStr)
        {
            JwtUserInfo jwtUserInfo = new JwtUserInfo();
            var jwtHandler = new JwtSecurityTokenHandler();

            if (!string.IsNullOrEmpty(jwtStr) && jwtHandler.CanReadToken(jwtStr))
            {
                //将JWT字符读取到JWT对象
                JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(jwtStr);

                //获取JWT中的用户信息
                jwtUserInfo.Uid = jwtToken.Id.ObjToLong();

                //获取JWT中的用户角色
                object role;
                jwtToken.Payload.TryGetValue(ClaimTypes.Role, out role);

                if (role != null && role is string)
                {
                    jwtUserInfo.Roles = new List<string>() { role.ToString() };
                }
                else if (role != null)
                {
                    jwtUserInfo.Roles = JsonHelper.JsonToObj<List<string>>(role.ToString());
                }
                else
                {
                    jwtUserInfo.Roles = new List<string>();
                }
            }
            return jwtUserInfo;
        }
    }
}
