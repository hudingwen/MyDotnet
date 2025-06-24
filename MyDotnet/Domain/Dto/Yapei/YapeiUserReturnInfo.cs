namespace MyDotnet.Domain.Dto.Yapei
{
    public class YapeiUserReturnInfo
    {
        public int status {  get; set; }
        public List<YapeiUserReturnInfoData> data { get; set; }
        public YapeiLoginReturnInfoError error { get; set; }
    }
}
