using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDotnet.Domain.Dto.System
{
    /// <summary>
    /// JWT用户信息
    /// </summary>
    public class JwtUserInfo
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public long Uid { get; set; }

        /// <summary>
        /// 部门id
        /// </summary>
        public long DepartmentId { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public List<string> Roles { get; set; }
    }
}
