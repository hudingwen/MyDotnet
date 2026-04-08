
using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Trojan
{
    ///<summary>
    ///users自定义服务器
    ///</summary>
    [SugarTable("servers_cus", "users自定义服务器")]
    [Tenant("trojan")] //('代表是哪个数据库，名字是appsettings.json 的 ConnId')
    public partial class TrojanCusServers
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsNullable = false, IsPrimaryKey = true)]
        public long id { set; get; }
        /// <summary>
        /// 绑定用户(启用)
        /// </summary>
        public int userid { get; set; }
        /// <summary>
        /// 服务器名称
        /// </summary>
        public string servername { set; get; }
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string serveraddress { set; get; }
        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string serverremark { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool serverenable { get; set; }
        /// <summary>
        /// 是否面向所有用户
        /// </summary>
        public bool isAllUser { get; set; }
        /// <summary>
        /// 绑定用户
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<int> bindUsers { get; set; }
        /// <summary>
        /// 排除用户
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<int> excludeUsers { get; set; }
    }
}
