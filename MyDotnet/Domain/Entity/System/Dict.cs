using SqlSugar;

namespace MyDotnet.Domain.Entity.System
{
    ///<summary>
    ///字典表
    ///</summary>
    public partial class Dict
    {
        /// <summary>
        /// 字典code
        /// </summary>
        [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = false)]
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
        /// 字典描述
        /// </summary>
        public string description { set; get; }
    }
}
