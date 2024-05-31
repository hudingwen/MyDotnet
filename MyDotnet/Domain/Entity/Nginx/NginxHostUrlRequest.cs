using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Nginx
{
    /// <summary>
    /// Nginx域名下的地址统计
    /// </summary>
    public class NginxHostUrlRequest : RootEntityTkey<long>
    {
        /// <summary>
        /// 统计url
        /// </summary>
        [SugarColumn(Length = 300, IsNullable = true)]
        public string url { get; set; }
        /// <summary>
        /// 访问次数
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int requestCount { get; set; }


    }
}
