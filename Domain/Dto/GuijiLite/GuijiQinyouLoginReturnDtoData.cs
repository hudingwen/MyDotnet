using Org.BouncyCastle.Asn1.Ocsp;

namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiQinyouLoginReturnDtoData
    {
        public string token { get; set; }

        public long expireTime { get; set; }

        public bool first { get; set; }

        public string unionId { get; set; }
    }
}
