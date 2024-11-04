
using System;
using System.Linq;
using System.Text;
using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Trojan
{
    ///<summary>
    ///users自定义URL服务器排除用户
    ///</summary>
    [SugarTable("servers_url_users_exclude", "users自定义URL服务器排除用户")]
    [Tenant("trojan")] //('代表是哪个数据库，名字是appsettings.json 的 ConnId')
    public partial class TrojanUrlServersUsersExclude : BaseEntity
    {
        /// <summary>
        /// 关联用户id
        /// </summary>
        public int userid { set; get; }
        /// <summary>
        /// 关联服务器id
        /// </summary>
        public long serverid { set; get; }
    }
}
