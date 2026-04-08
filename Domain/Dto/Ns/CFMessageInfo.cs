using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDotnet.Domain.Dto.Ns
{
    public class CFMessageInfo
    {
        public CFResult result { get; set; }
        public bool success { get; set; }
    }
    public class CFMessageListInfo
    {
        public List<CFResult> result { get; set; }
        public bool success { get; set; }
    }
    public class CFResult
    {
        public string id { get; set; }
        public string zone_id { get; set; }
        public string zone_name { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string content { get; set; }
        public bool proxiable { get; set; }
        public bool proxied { get; set; }
        public int ttl { get; set; }
        public bool locked { get; set; }
        public string comment { get; set; }
    }
    public class CFAddMessageInfo
    {
        public string content { get; set; }
        public string name { get; set; }
        public bool proxied { get; set; }
        public string type { get; set; }
        public string comment { get; set; }
        public int ttl { get; set; }
    }
}
