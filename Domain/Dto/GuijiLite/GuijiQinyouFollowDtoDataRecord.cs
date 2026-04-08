using AppStoreConnect.Model;
using Org.BouncyCastle.Asn1.Ocsp;

namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiQinyouFollowDtoDataRecord
    {

        public string sharerNickName { get; set; }
        public string sharerOneId { get; set; }
        public string? sharerAvatar { get; set; }
        public string followTime { get; set; }
        public int specialFollow { get; set; }
        public string source { get; set; }
        public GuijiQinyouFollowDtoDataRecordApp appInfo { get; set; }
        public bool moreDeviceFlag { get; set; }
        public string followRelationId { get; set; }
        public string? sharerRemark { get; set; }


    }
}
