namespace MyDotnet.Domain.Dto.Outai
{
    public class OutaiLoginReturnDto
    {
        public string msg { get; set; }
        /// <summary>
        /// -1 错误  1 正确
        /// </summary>
        public int state { get; set; }
        public string token { get; set; }
        public DateTime tokenExpire { get; set; }

        public OutaiLoginReturnDtoUser user { get;set; }

    }
}
