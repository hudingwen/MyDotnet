namespace MyDotnet.Domain.Dto.Yapei
{
    public class YapeiLoginReturnInfo
    {
        public int status {  get; set; }
        public YapeiLoginReturnInfoData data { get; set; }
        public YapeiLoginReturnInfoError error { get; set; }
    }
}
