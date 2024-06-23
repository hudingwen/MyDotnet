namespace MyDotnet.Domain.Dto.Ns
{
    /// <summary>
    /// 血糖上传dto
    /// </summary>
    public class NsUploadBloodInfo
    {
        public double sgv { get; set; }
        public long date { get; set; }
        public string type { get; set; } = "sgv";


        public string direction { get; set; } = "Flat";
        public string device { get; set; } = "aiyundiy";
        public string dateString { get { return DateTimeOffset.FromUnixTimeMilliseconds(date).UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); }  } 

    }
}
