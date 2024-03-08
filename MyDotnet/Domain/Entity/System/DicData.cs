using MyDotnet.Domain.Entity.Base;

namespace MyDotnet.Domain.Entity.System
{
    /// <summary>
    /// 字典数据表(多数据)
    /// </summary>
    public class DicData: BaseEntity
    {
        /// <summary>
        /// 父级字典code
        /// </summary>
        public string pCode { set; get; }
        /// <summary>
        /// 字典code
        /// </summary>
        public string code { set; get; }
        /// <summary>
        /// 字典名称
        /// </summary>
        public string name { set; get; }
        /// <summary>
        /// 字典内容
        /// </summary>
        public string content { set; get; }
        /// <summary>
        /// 字典内容2
        /// </summary>
        public string content2 { set; get; }
        /// <summary>
        /// 字典内容3
        /// </summary>
        public string content3 { set; get; }
        /// <summary>
        /// 字典描述
        /// </summary>
        public string description { set; get; }

        /// <summary>
        /// 排序
        /// </summary>
        public int codeOrder { get; set; }
    }
}
