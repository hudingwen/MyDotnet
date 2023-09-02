
using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Trojan
{
    ///<summary>
    ///users自定义URL服务器
    ///</summary>
    [SugarTable("users_url", "users自定义URL服务器")]
    [Tenant("trojan")] //('代表是哪个数据库，名字是appsettings.json 的 ConnId')
    public partial class TrojanUrlServers
    {

        [SugarColumn(IsNullable = false, IsPrimaryKey = true)]
        public long id { set; get; }
        public int userid { get; set; }
        public string servername { set; get; }
        public string serveraddress { set; get; }
        [SugarColumn(IsNullable = true)]
        public string serverremark { get; set; }
        public bool serverenable { get; set; }
    }
}
