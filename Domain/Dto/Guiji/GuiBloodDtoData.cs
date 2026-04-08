namespace MyDotnet.Domain.Dto.Guiji
{
    public class GuiBloodDtoData
    {
        public string id {  get; set; }
        public string userId { get; set; }
        public string followedUserId { get; set; }
        public GuiBloodDtoDataFollowInfo followedUserInfo { get; set; }
        public string source { get; set; }
        public string followTime {  get; set; }
        public string status {  get; set; }
        public string notes { get; set; }
        public GuiBloodDtoDataLikes likesStatistic { get; set; }
        public GuiBloodDtoDataOtherFollow otherInfo { get; set; }
        public GuiBloodDtoDataOtherInfo followedOtherInfo { get; set; }
        public GuiBloodDtoDataDevice followedDeviceGlucoseDataPO { get; set; }
    }
}
