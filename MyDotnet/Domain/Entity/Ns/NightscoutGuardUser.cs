using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Ns
{
    /// <summary>
    /// 监护用户
    /// </summary>
    public class NightscoutGuardUser : BaseEntity
    {
        /// <summary>
        /// 绑定的用户名称
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string name { get; set; }
        /// <summary>
        /// 绑定的ns
        /// </summary>
        public long nid { get; set; }
        /// <summary>
        /// 绑定的ns名称
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string nidName { get; set; }
        /// <summary>
        /// 绑定的ns网址(冗余)
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string nidUrl { get; set; }
        /// <summary>
        /// 绑定监护账号
        /// </summary>
        public long gid{ get; set; }
        /// <summary>
        /// 绑定监护账号名称
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string gidName { get; set; }
        /// <summary>
        /// 绑定监护用户
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true)]
        public string uid { get; set; }
        /// <summary>
        /// 绑定监护用户名称
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string uidName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string remark { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime startTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime endTime { get; set; }
        /// <summary>
        /// 刷新时间(上次获取时间)
        /// </summary>
        public DateTime refreshTime { get; set; }
        /// <summary>
        /// Nightscout token主键
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string nsTokenId { get; set; }
        /// <summary>
        /// Nightscout token名称(英文 10最大10位)
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string nsTokenName { get; set; }
        /// <summary>
        /// Nightscout token(实际用)
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string nsToken { get; set; }
    }
}
