
using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Ns
{
    /// <summary>
    /// 血糖名言/广告
    /// </summary>
    public class NightscoutBanner : BaseEntity
    {

        /// <summary>
        /// 标题
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string title { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [SugarColumn(Length = 255, IsNullable = true)]
        public string content { get; set; }
    }
}
