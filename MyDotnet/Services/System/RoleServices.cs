using MyDotnet.Domain.Entity.System;
using MyDotnet.Repository;

namespace MyDotnet.Services.System
{
    /// <summary>
    /// RoleServices
    public class RoleServices : BaseServices<Role>
    {
        public RoleServices(BaseRepository<Role> baseRepository) : base(baseRepository)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async Task<Role> SaveRole(string roleName)
        {
            Role role = new Role(roleName);
            Role model = new Role();
            var userList = await Dal.Query(a => a.Name == role.Name && a.Enabled);
            if (userList.Count > 0)
            {
                model = userList.FirstOrDefault();
            }
            else
            {
                var id = await Dal.Add(role);
                model = await Dal.QueryById(id);
            }

            return model;

        }
        public async Task<string> GetRoleNameByRid(int rid)
        {
            return (await Dal.QueryById(rid))?.Name;
        }
    }
}
