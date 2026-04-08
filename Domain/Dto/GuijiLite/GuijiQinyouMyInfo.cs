using Org.BouncyCastle.Asn1.Ocsp;

namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiQinyouMyInfo
    {
        public long timestamp { get; set; }

        public int code { get; set; }

        public string msg { get; set; }

        public GuijiQinyouMyInfoData data { get; set; }

        public bool success { get; set; }

    }
}
