using MyDotnet.Domain.Entity.Base;
using SqlSugar;
using System;


namespace MyDotnet.Domain.Entity.System
{
    ///<summary>
    /// 部门表
    ///</summary>
    public class Department : BaseEntity
    {
        /// <summary>
        /// 部门关系编码
        /// </summary>
        public string CodeRelationship { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 负责人
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string Leader { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int OrderSort { get; set; } = 0;
        /// <summary>
        /// 上级部门
        /// </summary>
        public long Pid { get; set; }

        /// <summary>
        /// 上级部门名称
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string PidName { get; set; }
        /// <summary>
        /// 子部门
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<Department> children { get; set; } = new List<Department>();
        /// <summary>
        /// 父级
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<long> PidArr { get; set; }
    }
}