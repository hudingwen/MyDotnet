namespace MyDotnet.Domain.Dto.Ns
{
    /// <summary>
    /// 血糖上传dto
    /// </summary>
    public class NsUploadBloodInfo
    {
        public double sgv { get; set; }
        public long date { get; set; }
        public string direction { get; set; }
        public string type { get; set; } = "sgv";
    }
}
