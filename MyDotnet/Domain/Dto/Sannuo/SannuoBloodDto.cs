namespace MyDotnet.Domain.Dto.Sannuo
{
    public class SannuoBloodDto
    {
        public int code { get; set; }
        public bool success { get; set; }
        public List<SannuoBloodDtoData> data { get; set; }
        public string msg { get; set; }
    }
}
