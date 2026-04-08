namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiLiteFollowDtoDataRecord
    {

        public string id { get; set; }
        public string userId { get; set; }
        public string followedUserId { get; set; }

        public GuijiLiteFollowDtoDataRecordFollow followedUserInfo { get; set; }
    }
}
