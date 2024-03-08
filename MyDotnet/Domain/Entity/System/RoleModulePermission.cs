using MyDotnet.Domain.Entity.Base;
using SqlSugar;
using System;

namespace MyDotnet.Domain.Entity.System
{
    /// <summary>
    /// 按钮跟权限关联表
    /// </summary>
    public class RoleModulePermission : BaseEntity
    {
        public RoleModulePermission()
        {
            //this.Role = new Role();
            //this.Module = new Module();
            //this.Permission = new Permission();
        }
        /// <summary>
        /// 角色ID
        /// </summary>
        public long RoleId { get; set; }
        /// <summary>
        /// 菜单ID
        /// </summary>
        public long ModuleId { get; set; }
        /// <summary>
        /// api ID
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public long PermissionId { get; set; }

        // 下边三个实体参数，只是做传参作用，所以忽略下
        [SugarColumn(IsIgnore = true)]
        public Role Role { get; set; }
        [SugarColumn(IsIgnore = true)]
        public Modules Module { get; set; }
        [SugarColumn(IsIgnore = true)]
        public Permission Permission { get; set; }
    }
}
