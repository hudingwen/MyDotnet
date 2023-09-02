using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Services.System;

namespace MyDotnet.Controllers.System
{
    /// <summary>
    /// 用户角色关系
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class UserRoleController : Controller
    {
        public SysUserInfoServices _sysUserInfoServices;
        public UserRoleServices _userRoleServices;
        public RoleServices _roleServices;
        public IMapper _mapper;

        public UserRoleController(SysUserInfoServices sysUserInfoServices
            , UserRoleServices userRoleServices
            , RoleServices roleServices
            , IMapper mapper
            )
        {
            _sysUserInfoServices = sysUserInfoServices;
            _userRoleServices = userRoleServices;
            _roleServices = roleServices;
            _mapper = mapper;
        }

        /// <summary>
        /// 新建用户
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="loginPwd"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<SysUserInfoDto>> AddUser(string loginName, string loginPwd)
        {
            var userInfo = await _sysUserInfoServices.SaveUserInfo(loginName, loginPwd);
            return new MessageModel<SysUserInfoDto>()
            {
                success = true,
                msg = "添加成功",
                response = _mapper.Map<SysUserInfoDto>(userInfo)
            };
        }

        /// <summary>
        /// 新建Role
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<Role>> AddRole(string roleName)
        {
            return new MessageModel<Role>()
            {
                success = true,
                msg = "添加成功",
                response = await _roleServices.SaveRole(roleName)
            };
        }

        /// <summary>
        /// 新建用户角色关系
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<UserRole>> AddUserRole(long uid, long rid)
        {
            return new MessageModel<UserRole>()
            {
                success = true,
                msg = "添加成功",
                response = await _userRoleServices.SaveUserRole(uid, rid)
            };
        }

    }
}
