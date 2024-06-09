namespace MyDotnet.Domain.Dto.Apple
{
    /// <summary>
    /// 设备查询返回列表dto
    /// </summary>
    public class DevicesListDto
    {
        public List<DevicesReturnAddData> data { get; set; }
        public AppleLinks links { get; set; }
        /// <summary>
        /// 分页信息
        /// </summary>
        public AppleMeta meta { get; set; }
    }
}
