namespace MyDotnet.Domain.Dto.Weitai1
{
    public class Weitai1FollowDtoContentRecord
    {

        public string emergeState { get; set; }
        public string isChildAccount { get; set; }
        public string pushState { get; set; }
        public string source { get; set; }
        public string wxState { get; set; }
        public string id { get; set; }
        public string userAlias { get; set; }
        public Weitai1FollowDtoContentRecordUser user { get; set; }
        public Weitai1FollowDtoContentRecordDevice device { get; set; }
        public Weitai1FollowDtoContentRecordHealth userHealthTarget { get; set; }
        public string hide { get; set; }
    }
}
