namespace MyDotnet.Domain.Dto.Outai
{
    public class OutaiFamilyDto
    {

        public string msg { get; set; }
        /// <summary>
        /// -1 错误  1 正确
        /// </summary>
        public int state { get; set; }
        public List<OutaiFamilyDtoList> associateFriendList { get; set; }
    }
}
