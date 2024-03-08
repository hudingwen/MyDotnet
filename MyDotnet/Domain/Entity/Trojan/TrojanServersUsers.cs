
using System;
using System.Linq;
using System.Text;
using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Trojan
{
    ///<summary>
    ///Trojan服务器关联用户
    ///</summary>
    [SugarTable("servers_users", "Trojan服务器关联用户")]
    [Tenant("trojan")] //('代表是哪个数据库，名字是appsettings.json 的 ConnId')
    public partial class TrojanServersUsers : BaseEntity
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
