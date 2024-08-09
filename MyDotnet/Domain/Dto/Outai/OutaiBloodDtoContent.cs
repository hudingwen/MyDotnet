namespace MyDotnet.Domain.Dto.Outai
{
    public class OutaiBloodDtoContent
    {
        public DateTime endTime { get; set; }
        public double latestData { get; set; }
        public string latestDataTrend { get; set; }
        public DateTime latestRecordDate { get; set; }
        public int remainDays { get; set; }
        public int remainHours { get; set; }
        public double tirScale { get; set; } 
        public OutaiBloodDtoContentRecord bloodSugarRecords { get; set; }
    }
}
