namespace MyDotnet.Domain.Dto.Guiji
{
    public class GuiBloodDto
    {
        public string timestamp { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public GuiBloodDtoData data { get; set; }
        public string errorData { get; set; }
        public bool success { get; set; }
    }
}
