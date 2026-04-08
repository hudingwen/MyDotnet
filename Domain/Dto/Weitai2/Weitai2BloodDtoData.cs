namespace MyDotnet.Domain.Dto.Weitai2
{
    public class Weitai2BloodDtoData
    {
        public string cgmRecordId { get; set; }
        public string frontRecordId { get; set; }
        public string userId { get; set; }
        public string sensorId { get; set; }
        public string autoIncrementColumn { get; set; }
        public int timeOffset { get; set; }
        public DateTime appTime { get; set; }
        public string appTimeZone { get; set; }
        public int dstOffset { get; set; }
        public double glucose { get; set; }
        public string direction { get; set; }
        public int status { get; set; }
        public int quality { get; set; }
        public int glucoseIsValid { get; set; }
        public string rawOne { get; set; }
        public string rawTwo { get; set; }
        public string rawVc { get; set; }
        public string rawIsValid { get; set; }
        public string eventWarning { get; set; }
        public DateTime appCreateTime { get; set; }
        public string trendValue { get; set; }
        public string appTimeOffset { get; set; }
        public string deviceStatus { get; set; }
        public string smooth { get; set; }
        public string smoothState { get; set; }
    }
}
