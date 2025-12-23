using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.System
{
    /// <summary>
    /// 字典类型表(单数据)
    /// </summary>
    public class DicType: BaseEntity
    {
        /// <summary>
        /// 字典code
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string code { set; get; }
        /// <summary>
        /// 字典名称
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string name { set; get; }
        /// <summary>
        /// 字典内容
        /// </summary>
        [SugarColumn(Length = 300, IsNullable = true)]
        public string content { set; get; }
        /// <summary>
        /// 字典内容2
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string content2 { set; get; }
        /// <summary>
        /// 字典内容3
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string content3 { set; get; }
        /// <summary>
        /// 字典内容4
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string content4 { set; get; }
        /// <summary>
        /// 字典内容5
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string content5 { set; get; }
        /// <summary>
        /// 字典描述
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string description { set; get; }
    }
}
