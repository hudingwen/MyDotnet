using MyDotnet.Domain.Entity.System;
using MyDotnet.Repository;

namespace MyDotnet.Services.System
{
    /// <summary>
    /// UserRoleServices
    /// </summary>	
    public class UserRoleServices : BaseServices<UserRole>
    {
        public UserRoleServices(BaseRepository<UserRole> baseRepository) : base(baseRepository)
        {
        }

        /// <summary>
        /// 保存用户角色
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        public async Task<UserRole> SaveUserRole(long uid, long rid)
        {
            UserRole userRole = new UserRole(uid, rid);

            UserRole model = new UserRole();
            var userList = await Dal.Query(a => a.UserId == userRole.UserId && a.RoleId == userRole.RoleId);
            if (userList.Count > 0)
            {
                model = userList.FirstOrDefault();
            }
            else
            {
                var id = await Dal.Add(userRole);
                model = await Dal.QueryById(id);
            }

            return model;

        }

    }
}
