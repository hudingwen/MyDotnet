namespace MyDotnet.Domain.Dto.Weitai2
{
    public class Weitai2LoginReturnDtoData
    {
        public string appId { get; set; }
        public string project { get; set; }
        public string identity { get; set; }
        public string userId { get; set; }
        public string username { get; set; }
        public string token { get; set; }
        public DateTime tokenExpire { get; set; }
        public string other { get; set; }
    }
}
