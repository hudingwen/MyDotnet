namespace MyDotnet.Domain.Dto.Guiji
{
    public class GuiLoginReturnDto
    {
        public string timestamp { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public GuiLoginReturnDtoData data { get; set; }
        public string errorData { get; set; }
        public bool success { get; set; }   
    }
}
