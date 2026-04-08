using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Hospital
{
    /// <summary>
    /// 学习目录
    /// </summary>
    public class StudyCategory:RootEntityTkey<long>
    {
        /// <summary>
        /// 目录父级id
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public long categoryParentId { get; set; }
        /// <summary>
        /// 目录名称
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string categoryName { get; set; }
        /// <summary>
        /// 目录排序
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int categorySort { get; set; }
        /// <summary>
        /// 目录描述
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string categoryDescription { get; set; }

    }
}
