namespace MyDotnet.Domain.Dto.Sannuo
{
    public class SannuoBloodDtoData
    {
        public string userId { get; set; }
        public string deviceSn { get; set; }
        public string dataSn { get; set; }
        public double value { get; set; }
        public long time { get; set; }
        /// <summary>
        /// 转换时间
        /// </summary>
        public DateTime parsTime { get; set; }
        /// <summary>
        /// 趋势
        /// </summary>
        public string direction { get; set; }

    }
}
