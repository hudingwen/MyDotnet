namespace MyDotnet.Domain.Dto.Guard
{
    public class GuardBloodInfo
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime time { get; set; }
        /// <summary>
        /// 血糖
        /// </summary>
        public double blood { get; set; }
        /// <summary>
        /// 趋势
        /// </summary>
        public string trend { get; set; }
        /// <summary>
        /// 类型(微泰1用)
        /// </summary>
        public string type { get; set; }
    }
}
