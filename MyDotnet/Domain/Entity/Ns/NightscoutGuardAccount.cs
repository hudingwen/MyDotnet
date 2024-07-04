using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Ns
{
    /// <summary>
    /// 监护账号
    /// </summary>
    public class NightscoutGuardAccount : BaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string name { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string remark { get; set; }
        /// <summary>
        /// 登录账号
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string loginName { get; set; }
        /// <summary>
        /// 登录密码
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string loginPass { get; set; }
        /// <summary>
        /// 登录成功后的id(有些需要)
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string loginId { get; set; }
        /// <summary>
        /// 当前登录token
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true)]
        public string token { get; set; }
        /// <summary>
        /// token过期时间
        /// </summary>
        public DateTime tokenExpire { get; set; }
        /// <summary>
        /// 监护类型  100-硅基 200-三诺 300-微泰1 400-微泰2
        /// </summary>
        public string guardType { get; set; }
        /// <summary>
        /// 是否有效
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool isEffect { get; set; }
    }
}
