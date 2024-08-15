namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiLiteUserBloodDtoDataPO
    {
        public string userId { get; set; }
        public string deviceId { get; set; }
        public string deviceEnableTime { get; set; }
        public string deviceStatus { get; set; }
        public string deviceAlarmStatus { get; set; }
        public double latestGlucoseValue { get; set; }
        public int bloodGlucoseTrend { get; set; }
        public long latestGlucoseTime { get; set; }
        public string deviceLastTime { get; set; }
        public List<GuijiLiteUserBloodDtoDataPOBlood> glucoseInfos { get; set; }
        public DateTime time { get; set; }

    }
}
