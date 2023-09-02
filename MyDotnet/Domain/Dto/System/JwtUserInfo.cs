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
        /// ID
        /// </summary>
        public long Uid { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public List<string> Roles { get; set; }
    }
}
