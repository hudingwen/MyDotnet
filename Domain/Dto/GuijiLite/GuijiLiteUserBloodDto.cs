namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiLiteUserBloodDto
    {
        public string timestamp { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public bool success { get; set; }
        public string errorData { get; set; }

        public GuijiLiteUserBloodDtoData data { get; set; }

    }
}
