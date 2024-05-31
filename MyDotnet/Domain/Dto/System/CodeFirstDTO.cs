namespace MyDotnet.Domain.Dto.System
{
    public class CodeFirstDTO
    {
        /// <summary>
        /// 数据库id
        /// </summary>
        public string connId { get; set; }

        /// <summary>
        /// 需要生成的表名列表
        /// </summary>
        public string[] lsTableNames { get; set; }

    }
}
