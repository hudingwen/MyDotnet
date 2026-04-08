using MyDotnet.Domain.Dto.Trojan;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Trojan
{
    ///<summary>
    ///Trojan用户
    ///</summary>
    [SugarTable("users", "Trojan用户表")]
    [Tenant("trojan")] //('代表是哪个数据库，名字是appsettings.json 的 ConnId')
    public partial class TrojanUsers
    {

        [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = true)]
        public int id { set; get; }
        public string username { set; get; }
        public string password { set; get; }
        public long quota { set; get; }
        public ulong download { set; get; }
        public ulong upload { set; get; }
        public string passwordshow { set; get; }
        [SugarColumn(IsNullable = true)]
        public long CreateId { get; set; }
        [SugarColumn(IsNullable = true)]
        public string CreateBy { get; set; }
        [SugarColumn(IsNullable = true)]
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 历史流量记录
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<TrojanUseDetailDto> useList { get; set; }
        /// <summary>
        /// 绑定服务器
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<long> serverIds { get; set; } 
        /// <summary>
        /// 排除服务器
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<long> serverIdsExclude { get; set; }


        /// <summary>
        /// 绑定Cus服务器
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<long> serverCusIds { get; set; }
        /// <summary>
        /// 排除Cus服务器
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<long> serverCusIdsExclude { get; set; }


        /// <summary>
        /// 绑定Url服务器
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<long> serverUrlIds { get; set; }
        /// <summary>
        /// 排除Url服务器
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<long> serverUrlIdsExclude { get; set; }
    }
}
