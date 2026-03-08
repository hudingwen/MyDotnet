using Org.BouncyCastle.Asn1.Ocsp;

namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiQinyouFollowDtoData
    {

        public List<GuijiQinyouFollowDtoDataRecord> records { get; set; }
        public int currentPage { get; set; }
        public int pageSize { get; set; }
        public int totalPage { get; set; }
        public int total { get; set; }


    }
}
