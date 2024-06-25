namespace MyDotnet.Domain.Dto.Guiji
{
    public class GuiBloodDtoDataDevice
    {
        public string userId { get; set; }
        public string deviceId { get; set; }
        public string deviceEnableTime { get; set; }
        public string deviceStatus { get; set; }
        public string deviceAlarmStatus { get; set; }
        public double latestGlucoseValue { get; set; }
        public int bloodGlucoseTrend { get; set; }
        public long latestGlucoseTime { get; set; }
        public DateTime time { get; set; }
        public string deviceLastTime { get; set; }
        public List<GuiBloodDtoDataDeviceBlood> glucoseInfos { get; set; }
        //public object target { get; set; }
        public string deviceName { get; set; }






        
    }
}
