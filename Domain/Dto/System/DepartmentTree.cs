using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDotnet.Domain.Dto.System
{
    public class DepartmentTree
    {
        public long value { get; set; }
        public long Pid { get; set; }
        public string label { get; set; }
        public int order { get; set; }
        public bool disabled { get; set; }
        public List<DepartmentTree> children { get; set; }
    }
}
