namespace MyDotnet.Helper.Dto
{
    public class HttpResult
    {
        public int StatusCode { get; set; }
        public string Body { get; set; }
        public bool Success { get; set; }
    }
}
