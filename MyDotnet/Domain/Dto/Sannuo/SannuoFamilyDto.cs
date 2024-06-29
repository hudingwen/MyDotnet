namespace MyDotnet.Domain.Dto.Sannuo
{
    public class SannuoFamilyDto
    {
        public int code { get; set; }
        public bool success { get; set; }
        public List<SannuoFamilyDtoData> data { get; set; }
        public string msg { get; set; }
    }
}
