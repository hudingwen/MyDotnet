namespace MyDotnet.Domain.Dto.Apple
{
    /// <summary>
    /// 描述文件查询返回列表dto
    /// </summary>
    public class ProfilesListDto
    {
        public List<ProfilesReturnAddData> data { get; set; }
        public AppleLinks links { get; set; }
        /// <summary>
        /// 分页信息
        /// </summary>
        public AppleMeta meta { get; set; }
    }
}
