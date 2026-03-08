using Org.BouncyCastle.Asn1.Ocsp;

namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiQinyouUserBloodDtoDataDeviceInfo
    {
        public string deviceId { get; set; }
        public int remaining { get; set; }
        public string latestValue { get; set; }
        public double latestValueFormat { get; set; }
        public string latestTime { get; set; }
        public DateTime latestTimeFormat { get; set; }
        public string name { get; set; }
        public string blueToothNum { get; set; }
        public int tir { get; set; }
        public int status { get; set; }
        public int latestDataStatus { get; set; }
        public int frequency { get; set; }
        public List<GuijiQinyouUserBloodDtoDataDeviceInfoBlood> glucoseInfos { get; set; }

    }
}
