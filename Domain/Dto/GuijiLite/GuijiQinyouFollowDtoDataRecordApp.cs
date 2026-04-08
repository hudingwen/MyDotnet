using AppStoreConnect.Model;
using Org.BouncyCastle.Asn1.Ocsp;

namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiQinyouFollowDtoDataRecordApp
    {

        public string sharerUserId { get; set; }
        public string appCode { get; set; }
        public bool enabled { get; set; }
        public bool rtGlucose { get; set; }
        public bool action { get; set; }
        public bool report { get; set; }
        public bool history { get; set; }
        public bool master { get; set; }
        public int totalLikes { get; set; }
        public bool todayLiked { get; set; }
        public string? latestValue { get; set; }
        public string? latestTime { get; set; }
        public int? latestDataStatus { get; set; }
        public int? frequency { get; set; }
        public int? deviceStatus { get; set; }
        public string? deviceId { get; set; }


    }
}
