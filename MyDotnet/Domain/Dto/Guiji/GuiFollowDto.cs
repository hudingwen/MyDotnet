namespace MyDotnet.Domain.Dto.Guiji
{
    public class GuiFollowDto
    {

        public string timestamp { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public GuiFollowDtoData data { get; set; }
        public string errorData { get; set; }
        public bool success { get; set; }
    }
}
