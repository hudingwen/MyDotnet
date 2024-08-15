namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiLiteUserBloodDtoData
    {
        public string id { get; set; }
        public string userId { get; set; }
        public string followedUserId { get; set; }
        public GuijiLiteUserBloodDtoDataFollow followedUserInfo { get; set; }

        public string source { get; set; }
        public string followTime { get; set; }
        public string status { get; set; }
        public string notes { get; set; }


        public GuijiLiteUserBloodDtoDataPO followedDeviceGlucoseDataPO { get; set; }
    }
}
