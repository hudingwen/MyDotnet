namespace MyDotnet.Domain.Dto.Weitai1
{
    public class Weitai1BloodDtoContentRecord
    {
        public string recordUuid { get; set; }
        public string recordIndex { get; set; }
        public string deviceId { get; set; }
        public string sensorId { get; set; }
        public int sensorIndex { get; set; }
        public string eventIndex { get; set; }
        public DateTime deviceTime { get; set; }
        public int eventType { get; set; }
        public double eventData { get; set; }
        public string direction { get; set; }
    }
}
