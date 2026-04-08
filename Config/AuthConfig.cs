using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Helper;
using System.Security.Claims;
using System.Text;

namespace MyDotnet.Config
{
    /// <summary>
    /// 认证授权管理
    /// </summary>
    public static class AuthConfig
    {

        //读取配置文件
        public static readonly string secret = ConfigHelper.GetValue(new string[] { "Audience", "Secret" });//密钥
        public static readonly string issuer = ConfigHelper.GetValue(new string[] { "Audience", "Issuer" });//发行人
        public static readonly string audience = ConfigHelper.GetValue(new string[] { "Audience", "Audience" });//订阅人
        public static readonly int expire = ConfigHelper.GetValue(new string[] { "Audience", "Expire" }).ObjToInt();//token过期时间单位/小时
        /// <summary>
        /// 认证授权配置
        /// </summary>
        /// <param name="services"></param>
        public static void SetAuth(this WebApplicationBuilder builder)
        {

            var keyByteArray = Encoding.UTF8.GetBytes(secret);
            var signingKey = new SymmetricSecurityKey(keyByteArray);
            var sigCreds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256); //创建密钥签名对象

            //添加认证
            builder.Services.AddAuthentication(options =>
            {
                //添加Bearer默认认证
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                //添加自定义权限消息
                options.DefaultChallengeScheme = nameof(ApiResponseHandler);
                options.DefaultForbidScheme = nameof(ApiResponseHandler);

            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,//缓冲时间
                    RequireExpirationTime = true,
                };
            }).AddScheme<AuthenticationSchemeOptions, ApiResponseHandler>(nameof(ApiResponseHandler), o => { });

            //权限注入
            var permission = new List<PermissionItem>();
            var permissionRequirement = new PermissionRequirement(
                "/api/denied",// 拒绝授权的跳转地址（目前无用）
                permission,
                ClaimTypes.Role,//基于角色的授权
                issuer,//发行人
                audience,//听众
                sigCreds,//签名凭据
                expiration: TimeSpan.FromSeconds(3600)//接口的过期时间
                );
            builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.AddScoped(typeof(AspNetUser));

            builder.Services.AddSingleton(permissionRequirement);

            JWTHelper.permissionRequirement = permissionRequirement;

            // 自定义授权策略
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(Permissions.Name, policy => policy.Requirements.Add(permissionRequirement));
            });
        }
    }
}
