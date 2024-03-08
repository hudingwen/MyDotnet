namespace MyDotnet.Domain.Dto.System
{
    /// <summary>
    /// 分页参数
    /// </summary>
    public class PaginationModel 
    {
        /// <summary>
        /// 当前页
        /// </summary>
        public int page { get; set; } = 1;
        /// <summary>
        /// 每页大小
        /// </summary>
        public int size { get; set; } = 10;
        /// <summary>
        /// 搜索关键词
        /// </summary>
        public string key { get; set; }
        /// <summary>
        /// 排序字段(例如:id desc,time asc)
        /// </summary>
        public string orderByFileds { get; set; }
        /// <summary>
        /// 查询条件( 例如:id = 1 and name = 小明)
        /// </summary>
        public string conditions { get; set; }
    }
}
