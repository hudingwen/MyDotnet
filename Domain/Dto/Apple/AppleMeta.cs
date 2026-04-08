namespace MyDotnet.Domain.Dto.Apple
{
    /// <summary>
    /// 分页信息
    /// </summary>
    public class AppleMeta
    {
       public AppleMetaPaging paging { get; set; }
    }
    /// <summary>
    /// 分页信息
    /// </summary>
    public class AppleMetaPaging
    {
        public int total {  get; set; }
        public int limit {  get; set; }
    }

}
