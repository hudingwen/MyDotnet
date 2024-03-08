
using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Trojan
{
    ///<summary>
    ///users自定义URL服务器
    ///</summary>
    [SugarTable("servers_url", "users自定义URL服务器")]
    [Tenant("trojan")] //('代表是哪个数据库，名字是appsettings.json 的 ConnId')
    public partial class TrojanUrlServers
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsNullable = false, IsPrimaryKey = true)]
        public long id { set; get; }
        public int userid { get; set; }
        /// <summary>
        /// 绑定用户id(启用)
        /// </summary>
        public string servername { set; get; }
        /// <summary>
        /// url地址
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
    }
}
