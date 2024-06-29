namespace MyDotnet.Domain.Dto.Sannuo
{
    public class SannuoSmsLoginReturnDto
    {

        public int code { get; set; }
        public bool success { get; set; }
        public string msg { get; set; }
        public string access_token { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime tokenExpire { get; set; }
        public string token_type { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public string tenant_id { get; set; }
        public string account_type { get; set; }
        public string real_name { get; set; }
        public int user_type { get; set; }
        public string person_id { get; set; }
        public string client_scope { get; set; }
        public string object_id { get; set; }
        public string account_id { get; set; }
        public string avatar_url { get; set; }
        public string phone { get; set; }
        public string nick_name { get; set; }
        public string account { get; set; }
        public string jti { get; set; }
    }
}
