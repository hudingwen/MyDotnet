using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Nginx
{
    /// <summary>
    /// Nginx域名统计
    /// </summary>
    public class NginxHostRequest:RootEntityTkey<long>
    {
        /// <summary>
        /// 统计host
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string host { get; set; }
        /// <summary>
        /// 访问次数
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int requestCount { get; set; }
        /// <summary>
        /// 统计日期
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime date { get; set; }
        /// <summary>
        /// 网址统计
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<NginxHostUrlRequest> urls { get; set; } = new List<NginxHostUrlRequest> { };

    }
}
