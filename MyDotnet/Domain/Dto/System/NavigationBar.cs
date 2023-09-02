namespace MyDotnet.Domain.Dto.System
{
    public class NavigationBar
    {
        public long id { get; set; }
        public long pid { get; set; }
        public int order { get; set; }
        public string name { get; set; }
        public bool IsHide { get; set; } = false;
        public bool IsButton { get; set; } = false;
        public string path { get; set; }
        public string Func { get; set; }
        public string iconCls { get; set; }
        public NavigationBarMeta meta { get; set; }
        public List<NavigationBar> children { get; set; }
    }
}
