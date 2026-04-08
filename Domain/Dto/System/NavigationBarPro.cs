using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDotnet.Domain.Dto.System
{
    public class NavigationBarPro
    {
        public long id { get; set; }
        public long parentId { get; set; }
        public int order { get; set; }
        public string name { get; set; }
        public bool IsHide { get; set; } = false;
        public bool IsButton { get; set; } = false;
        public string path { get; set; }
        public string component { get; set; }
        public string Func { get; set; }
        public string iconCls { get; set; }
        public NavigationBarMetaPro meta { get; set; }
    }
}
