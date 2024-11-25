
using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Ns
{
    /// <summary>
    /// 血糖客户管理
    /// </summary>
    public class NightscoutCustomer : BaseEntity
    {

        /// <summary>
        /// 客户名称
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string name { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [SugarColumn(Length = 255, IsNullable = true)]
        public string logo { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true)]
        public string tel { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 255, IsNullable = true)]
        public string remark { get; set; }
        /// <summary>
        /// 首页介绍
        /// </summary>
        [SugarColumn(IsNullable = true,ColumnDataType ="text")]
        public string introduce { get; set; }

        


    }
}
