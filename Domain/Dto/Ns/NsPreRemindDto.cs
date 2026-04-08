namespace MyDotnet.Domain.Dto.Ns
{
    /// <summary>
    /// ns过期信息查询
    /// </summary>
    public class NsPreRemindDto
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public long uid { get; set; }
        public string name { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime startTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime endTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string endTimeStr { get; set; }
        /// <summary>
        /// 续费地址
        /// </summary>
        public string payUrl { get; set; }
        /// <summary>
        /// 续费地址
        /// </summary>
        public string payText { get; set; }
        /// <summary>
        /// 显示文本
        /// </summary>
        public string showText { get; set; }
        /// <summary>
        /// 显示标题
        /// </summary>
        public string showTitle { get; set; }
        /// <summary>
        /// 是否可以展示过期信息
        /// </summary>
        public bool isCanShowExpire { get; set; }

    }
}
