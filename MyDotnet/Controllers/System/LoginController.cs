
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Common.Cache;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.System;
using System.Collections;

namespace MyDotnet.Controllers.System
{
    /// <summary>
    /// 登录管理【无权限】
    /// </summary>
    [Produces("application/json")]
    [Route("api/Login")]
    [AllowAnonymous]
    public class LoginController : BaseApiController
    {
        public SysUserInfoServices _sysUserInfoServices;
        public UserRoleServices _userRoleServices;
        public RoleServices _roleServices;
        public PermissionRequirement _requirement;
        public RoleModulePermissionServices _roleModulePermissionServices;
        public CodeService _codeService;
        public BaseServices<UserGoogleAuthenticator> _userGoogleAuthService;
        public DicService _dictService;

        public LoginController(SysUserInfoServices sysUserInfoServices
            , UserRoleServices userRoleServices
            , RoleServices roleServices 
            , PermissionRequirement requirement
            , RoleModulePermissionServices roleModulePermissionServices
            , CodeService codeService
            , BaseServices<UserGoogleAuthenticator> userGoogleAuthService
            , DicService dictService
            )
        {
            _sysUserInfoServices = sysUserInfoServices;
            _userRoleServices = userRoleServices;
            _roleServices = roleServices;
            _requirement = requirement;
            _roleModulePermissionServices = roleModulePermissionServices;
            _codeService = codeService;
            _userGoogleAuthService = userGoogleAuthService;
            _dictService = dictService;
        }
        /// <summary>
        /// 登录系统
        /// </summary>
        /// <param name="name">账号</param>
        /// <param name="pass">密码</param>
        /// <param name="key">验证码key</param>
        /// <param name="code">验证码值</param>
        /// <param name="authCode">2FA验证码</param>
        /// <returns></returns>
        [HttpGet]
        [Route("JWTToken3.0")]
        public async Task<MessageModel<TokenInfoViewModel>> GetJwtToken3(string name = "", string pass = "",string key="",string code="",string authCode="")

        {
            string jwtStr = string.Empty;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass))
                return Failed<TokenInfoViewModel>("认证失败");

            var codeDic = await _dictService.GetDicDataOne(SysAuthInfo.KEY, SysAuthInfo.login_code_enable);
            if ("1".Equals(codeDic.content))
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(code))
                    return Failed<TokenInfoViewModel>("认证失败");
                if (!_codeService.ValidCode(key, code))
                    return Failed<TokenInfoViewModel>("认证失败");
            }


            pass = MD5Helper.MD5Encrypt32(pass);

            var user = await _sysUserInfoServices.Dal.Query(d => d.LoginName == name && d.IsDeleted == false);
            if (user.Count == 1)
            {
                if (!user[0].LoginPWD.Equals(pass)) return Failed<TokenInfoViewModel>("认证失败");
                if (user[0].auth2faEnable)
                {
                    if(string.IsNullOrEmpty(authCode)) return Failed<TokenInfoViewModel>("认证失败");

                    var auth =  await _userGoogleAuthService.Dal.QueryById(user[0].auth2faId);
                    if (!GoogleAuthenticatorHelper.ValidateTwoFactorPIN(auth.key, authCode))
                    {
                        return Failed<TokenInfoViewModel>("认证失败");
                    }

                }
                var userRoles = await _sysUserInfoServices.GetUserRoleNameStr(name, pass);

                JwtUserInfo jwtUserInfo = new JwtUserInfo() { Uid = user[0].Id, Name = name, Roles = userRoles.Split(',').ToList() };
                var token = JWTHelper.IssueJwt(jwtUserInfo);

                TokenInfoViewModel msg = new TokenInfoViewModel { success = true, token = token, token_type = "Bearer", expires_in = _requirement.Expiration.TotalSeconds };
                return Success(msg, "认证成功");
            }
            else
            {
                return Failed<TokenInfoViewModel>("认证失败");
            }
        }
        /// <summary>
        /// 2FA验证
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Get2FAInfo")]
        public async Task<MessageModel<bool>> Get2FAInfo(string name)

        { 

            var user = await _sysUserInfoServices.Dal.Query(d => d.LoginName == name && d.IsDeleted == false);
            if (user.Count == 1 && user[0].auth2faEnable)
            {
                return Success(true);
            }
            else
            {
                return Success(false);
            }
        }
        /// <summary>
        /// 验证码验证
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCodeInfo")]
        public async Task<MessageModel<bool>> GetCodeInfo()
        {
            var codeDic = await _dictService.GetDicDataOne(SysAuthInfo.KEY, SysAuthInfo.login_code_enable);
            if ("1".Equals(codeDic.content))
            {
                return Success(true);
            }
            else
            {
                return Success(false);
            }
        }

        /// <summary>
        /// 测试 MD5 加密字符串
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Md5Password")]
        public string Md5Password(string password = "")
        {
            return MD5Helper.MD5Encrypt32(password);
        }
        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getCode")]
        public MessageModel<CodeDto> getCode()
        {
            var data = _codeService.CreateCode();
            return Success(data);
        }
    }
}