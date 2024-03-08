
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Helper;
using MyDotnet.Services.System;

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

        public LoginController(SysUserInfoServices sysUserInfoServices
            , UserRoleServices userRoleServices
            , RoleServices roleServices 
            , PermissionRequirement requirement
            , RoleModulePermissionServices roleModulePermissionServices
            )
        {
            _sysUserInfoServices = sysUserInfoServices;
            _userRoleServices = userRoleServices;
            _roleServices = roleServices;
            _requirement = requirement;
            _roleModulePermissionServices = roleModulePermissionServices;

        }

        #region 获取token的第1种方法

        /// <summary>
        /// 获取JWT的方法1
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Token")]
        public async Task<MessageModel<string>> GetJwtStr(string name, string pass)
        {
            string jwtStr = string.Empty;
            bool suc = false;
            //这里就是用户登陆以后，通过数据库去调取数据，分配权限的操作

            var user = await _sysUserInfoServices.GetUserRoleNameStr(name, MD5Helper.MD5Encrypt32(pass));
            if (user != null)
            {
                JwtUserInfo tokenModel = new JwtUserInfo { Uid = 1, Roles = new List<string> { user } };

                jwtStr = JWTHelper.IssueJwt(tokenModel);
                suc = true;
            }
            else
            {
                jwtStr = "login fail!!!";
            }

            return new MessageModel<string>()
            {
                success = suc,
                msg = suc ? "获取成功" : "获取失败",
                response = jwtStr
            };
        }


        /// <summary>
        /// 获取JWT的方法2：给Nuxt提供
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTokenNuxt")]
        public MessageModel<string> GetJwtStrForNuxt(string name, string pass)
        {
            string jwtStr = string.Empty;
            bool suc = false;
            //这里就是用户登陆以后，通过数据库去调取数据，分配权限的操作
            //这里直接写死了
            if (name == "admins" && pass == "admins")
            {
                JwtUserInfo tokenModel = new JwtUserInfo
                {
                    Uid = 1,
                    Roles = new List<string> { "Admin" }
                };

                jwtStr = JWTHelper.IssueJwt(tokenModel);
                suc = true;
            }
            else
            {
                jwtStr = "login fail!!!";
            }

            var result = new
            {
                data = new { success = suc, token = jwtStr }
            };

            return new MessageModel<string>()
            {
                success = suc,
                msg = suc ? "获取成功" : "获取失败",
                response = jwtStr
            };
        }

        #endregion


        /// <summary>
        /// 获取JWT的方法3：整个系统主要方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("JWTToken3.0")]
        public async Task<MessageModel<TokenInfoViewModel>> GetJwtToken3(string name = "", string pass = "")

        {
            string jwtStr = string.Empty;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass))
                return Failed<TokenInfoViewModel>("用户名或密码不能为空");

            pass = MD5Helper.MD5Encrypt32(pass);

            var user = await _sysUserInfoServices.Dal.Query(d => d.LoginName == name && d.LoginPWD == pass && d.IsDeleted == false);
            if (user.Count > 0)
            {
                var userRoles = await _sysUserInfoServices.GetUserRoleNameStr(name, pass);

                JwtUserInfo jwtUserInfo = new JwtUserInfo() { Uid = user[0].Id, Name = name, Roles = userRoles.Split(',').ToList() };
                var token = JWTHelper.IssueJwt(jwtUserInfo);

                //var data = await _roleModulePermissionServices.RoleModuleMaps();
                //var list = (from item in data
                //            where item.IsDeleted == false
                //            orderby item.Id
                //            select new PermissionItem
                //            {
                //                Url = item.Module?.LinkUrl,
                //                Role = item.Role?.Name.ObjToString(),
                //            }).ToList();

                //_requirement.Permissions = list;



                TokenInfoViewModel msg = new TokenInfoViewModel { success = true, token = token, token_type = "Bearer", expires_in = _requirement.Expiration.TotalSeconds };
                return Success(msg, "获取成功");
            }
            else
            {
                return Failed<TokenInfoViewModel>("认证失败");
            }
        }

        /// <summary>
        /// 请求刷新Token（以旧换新）
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("RefreshToken")]
        public async Task<MessageModel<TokenInfoViewModel>> RefreshToken(string token = "")
        {
            string jwtStr = string.Empty;

            if (string.IsNullOrEmpty(token))
                return Failed<TokenInfoViewModel>("token无效，请重新登录！");

            return Failed<TokenInfoViewModel>("认证失败，请重新登录");
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
    }
}