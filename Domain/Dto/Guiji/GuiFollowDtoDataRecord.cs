namespace MyDotnet.Domain.Dto.Guiji
{
    public class GuiFollowDtoDataRecord
    {
        public string id { get; set; }
        public string userId { get; set; }
        public string followedUserId { get; set; }
        public GuiFollowDtoDataRecordFollowInfo followedUserInfo { get; set; }


        public string source { get; set; }
        public string followTime { get; set; }
        public string status { get; set; }
        public string notes { get; set; }
        public GuiFollowDtoDataRecordOther otherInfo { get; set; } 
        public string followType { get; set; }
        public GuiFollowDtoDataRecordBlood followedDeviceGlucoseDataPO { get; set; }
        //public string permissions { get; set; }
        //public string friendCancelFollow { get; set; }
        //public string userFingerBloodVO { get; set; }
    }
}
