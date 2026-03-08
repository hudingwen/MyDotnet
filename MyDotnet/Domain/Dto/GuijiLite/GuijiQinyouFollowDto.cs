using Org.BouncyCastle.Asn1.Ocsp;

namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiQinyouFollowDto
    {

        public long timestamp { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public GuijiQinyouFollowDtoData data { get; set; }
        public object? errorData { get; set; }
        public bool success { get; set; }


    }
}
