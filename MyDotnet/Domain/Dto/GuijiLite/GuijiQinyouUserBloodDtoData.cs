using Org.BouncyCastle.Asn1.Ocsp;

namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiQinyouUserBloodDtoData
    {
        public GuijiQinyouUserBloodDtoDataSharInfo sharerInfo { get; set; }
        public GuijiQinyouUserBloodDtoDataDeviceInfo deviceInfo { get; set; }
        public bool rtGlucose { get; set; }
        public bool action { get; set; }
        public bool report { get; set; }
        public bool history { get; set; }

    }
}
