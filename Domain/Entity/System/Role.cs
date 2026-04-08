
using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.System
{
    /// <summary>
    /// 角色表
    /// </summary>
    public class Role : BaseEntity
    {
        public Role()
        {
            OrderSort = 1;
        }
        public Role(string name)
        {
            Name = name;
            Description = "";
            OrderSort = 1;

        } 
        /// <summary>
        /// 角色名
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string Name { get; set; }
        /// <summary>
        ///描述
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string Description { get; set; }
        /// <summary>
        ///排序
        /// </summary>
        public int OrderSort { get; set; }
        /// <summary>
        /// 自定义权限的部门ids
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string Dids { get; set; }
        /// <summary>
        /// 权限范围
        /// -1 无任何权限；1 自定义权限；2 本部门；3 本部门及以下；4 仅自己；9 全部；
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int AuthorityScope { get; set; } = -1;



    }
}
