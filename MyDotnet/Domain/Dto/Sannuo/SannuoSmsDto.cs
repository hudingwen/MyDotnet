namespace MyDotnet.Domain.Dto.Sannuo
{
    public class SannuoSmsDto
    {
        public int validateSmsType { get; set; } = 1;
        /// <summary>
        /// 手机号
        /// </summary>
        public string phone { get; set; } = "";
        /// <summary>
        /// 手机号
        /// </summary>
        public string nonce { get; set; } = "";
        public long timestamp { get; set; } = 1718506329072;
        public string signature { get; set; } = "b46b2fa00904ba3707aa1e3549868e0e";
    }
}
