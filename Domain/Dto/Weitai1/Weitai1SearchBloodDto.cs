namespace MyDotnet.Domain.Dto.Weitai1
{
    public class Weitai1SearchBloodDto
    {
        public Weitai1SearchBloodDtoThan greaterThan { get; set; } = new Weitai1SearchBloodDtoThan();
        public string authorizationId { get; set; }
        public string sortOrder { get; set; } = "DESC";
        public int pageSize { get; set; } = 100;
        public int currentPage { get; set; } = 1;
    }
}
