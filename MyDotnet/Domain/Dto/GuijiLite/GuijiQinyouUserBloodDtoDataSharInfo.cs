using Org.BouncyCastle.Asn1.Ocsp;

namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiQinyouUserBloodDtoDataSharInfo
    {
        public string sharerNickname { get; set; }
        public string? sharerRemark { get; set; }
        public string followTime { get; set; }
        public string? sharerAvatar { get; set; }
        public bool todayLiked { get; set; }
        public int totalLikes { get; set; }
        public string sharerUserId { get; set; }
        public string sharerOneId { get; set; }
        public string followRelationId { get; set; }
        public string appCode { get; set; }

    }
}
