namespace MyDotnet.Domain.Dto.GuijiLite
{
    public class GuijiLiteFollowDtoData
    {

        public int currentPage { get; set; }
        public int pageSize { get; set; }
        public int totalPage { get; set; }
        public int total { get; set; }


        public List<GuijiLiteFollowDtoDataRecord> records { get;set; }
    }
}
