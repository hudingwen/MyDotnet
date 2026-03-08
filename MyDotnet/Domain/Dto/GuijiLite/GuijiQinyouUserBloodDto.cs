using Org.BouncyCastle.Asn1.Ocsp;

namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiQinyouUserBloodDto
    {
        public long timestamp { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public GuijiQinyouUserBloodDtoData data { get; set; }
        public object? errorData { get; set; }
        public bool success { get; set; }

    }
}
