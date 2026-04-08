namespace MyDotnet.Domain.Dto.Guiji
{
    public class GuiFollowDtoDataRecordBlood
    {
        public string userId { get; set; }
        public string deviceId { get; set; }
        public string deviceEnableTime { get; set; }
        public string deviceStatus { get; set; }
        public string deviceAlarmStatus { get; set; }
        public string latestGlucoseValue { get; set; }
        public string bloodGlucoseTrend { get; set; }
        public string deviceLastTime { get; set; }
        public string latestGlucoseTime { get; set; }
    }
}
