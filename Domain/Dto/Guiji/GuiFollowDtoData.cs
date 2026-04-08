namespace MyDotnet.Domain.Dto.Guiji
{
    public class GuiFollowDtoData
    {
        public List<GuiFollowDtoDataRecord> records { get; set; }
        public int currentPage {  get; set; }   
        public int pageSize { get; set; }
        public int totalPages { get; set; }
        public int total { get; set; }
    }
}
