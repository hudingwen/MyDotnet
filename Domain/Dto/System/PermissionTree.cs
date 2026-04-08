using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDotnet.Domain.Dto.System
{
    public class PermissionTree
    {
        public long value { get; set; }
        public long Pid { get; set; }
        public string label { get; set; }
        public int order { get; set; }
        public bool isbtn { get; set; }
        public bool disabled { get; set; }
        public List<PermissionTree> children { get; set; }
        public List<PermissionTree> btns { get; set; }
    }
}
