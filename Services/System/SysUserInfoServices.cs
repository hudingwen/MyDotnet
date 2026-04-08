using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;

namespace MyDotnet.Services.System
{
    /// <summary>
    /// sysUserInfoServices
    /// </summary>	
    public class SysUserInfoServices : BaseServices<SysUserInfo>
    {
        public SysUserInfoServices(BaseRepository<SysUserInfo> baseRepository
            , BaseRepository<UserRole> userRoleRepository
            , BaseRepository<Role> roleRepository
            ) : base(baseRepository)
        {
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
        }

        public BaseRepository<UserRole> _userRoleRepository { get; set; }
        public BaseRepository<Role> _roleRepository { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="loginPwd"></param>
        /// <returns></returns>
        public async Task<SysUserInfo> SaveUserInfo(string loginName, string loginPwd)
        {
            SysUserInfo sysUserInfo = new SysUserInfo(loginName, loginPwd);
            SysUserInfo model = new SysUserInfo();
            var userList = await Dal.Query(a => a.LoginName == sysUserInfo.LoginName && a.LoginPWD == sysUserInfo.LoginPWD);
            if (userList.Count > 0)
            {
                model = userList.FirstOrDefault();
            }
            else
            {
                var id = await Dal.Add(sysUserInfo);
                model = await Dal.QueryById(id);
            }
            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<string> GetUserRoleNameStr(long uid)
        {
            string roleName = "";
            var user = await Dal.QueryById(uid);
            var roleList = await _roleRepository.Query();
            if (user != null)
            {
                var userRoles = await _userRoleRepository.Query(ur => ur.UserId == user.Id);
                if (userRoles.Count > 0)
                {
                    var arr = userRoles.Select(ur => ur.RoleId.ObjToString()).ToList();
                    var roles = roleList.Where(d => arr.Contains(d.Id.ObjToString()));

                    roleName = string.Join(',', roles.Select(r => r.Name).ToArray());
                }
            }
            return roleName;
        }
    }
}