using MyDotnet.Domain.Entity.System;
using MyDotnet.Repository;

namespace MyDotnet.Services.System
{
    /// <summary>
    /// 角色权限关系服务类
    /// </summary>	
    public class RoleModulePermissionServices : BaseServices<RoleModulePermission>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="baseRepository"></param>
        /// <param name="moduleRepository"></param>
        /// <param name="roleRepository"></param>
        public RoleModulePermissionServices(BaseRepository<RoleModulePermission> baseRepository
            , BaseRepository<Modules> moduleRepository
            , BaseRepository<Role> roleRepository
            ) : base(baseRepository)
        {
            _moduleRepository = moduleRepository;
            _roleRepository = roleRepository;
        }
        public BaseRepository<Modules> _moduleRepository { get; set; }
        public BaseRepository<Role> _roleRepository { get; set; }
        /// <summary>
        /// 获取全部 角色接口(按钮)关系数据
        /// </summary>
        /// <returns></returns>
        public async Task<List<RoleModulePermission>> GetRoleModule()
        {
            var roleModulePermissions = await Dal.Query(a => a.IsDeleted == false);
            var roles = await _roleRepository.Query(a => a.IsDeleted == false);
            var modules = await _moduleRepository.Query(a => a.IsDeleted == false);

            //var roleModulePermissionsAsync = base.Query(a => a.IsDeleted == false);
            //var rolesAsync = _roleRepository.Query(a => a.IsDeleted == false);
            //var modulesAsync = _moduleRepository.Query(a => a.IsDeleted == false);

            //var roleModulePermissions = await roleModulePermissionsAsync;
            //var roles = await rolesAsync;
            //var modules = await modulesAsync;


            if (roleModulePermissions.Count > 0)
            {
                foreach (var item in roleModulePermissions)
                {
                    item.Role = roles.FirstOrDefault(d => d.Id == item.RoleId);
                    item.Module = modules.FirstOrDefault(d => d.Id == item.ModuleId);
                }

            }
            return roleModulePermissions;
        }
        /// <summary>
        /// 批量更新菜单与接口的关系
        /// </summary>
        /// <param name="permissionId">菜单主键</param>
        /// <param name="moduleId">接口主键</param>
        /// <returns></returns>
        public async Task UpdateModuleId(long permissionId, long moduleId)
        {
            await Dal.Db.Updateable<RoleModulePermission>(it => it.ModuleId == moduleId).Where(
                it => it.PermissionId == permissionId).ExecuteCommandAsync();
        }
    }


}
