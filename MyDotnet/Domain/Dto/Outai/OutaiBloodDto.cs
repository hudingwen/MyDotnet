namespace MyDotnet.Domain.Dto.Outai
{
    public class OutaiBloodDto
    {
        public string msg { get; set; }
        /// <summary>
        /// -1 错误  1 正确
        /// </summary>
        public int state { get; set; }
        public OutaiBloodDtoContent content { get; set; }
    }
}
