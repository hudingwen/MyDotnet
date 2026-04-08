using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.System
{
    /// <summary>
    /// CDN文件使用记录表
    /// </summary>
    public class FileRecord : BaseEntity
    {

        /// <summary>
        /// 文件url
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false)]
        public string fileUrl { get; set; }
        /// <summary>
        /// 文件描述
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string fileDescription { get; set; }
        /// <summary>
        /// 文件备注
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string fileRemark { get; set; }
    }
}
