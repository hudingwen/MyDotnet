using System;
using System.Collections.Generic;
using System.Text;

namespace MyDotnet.Domain.Dto.Trojan
{
    /// <summary>
    /// 限制流量dto
    /// </summary>
    public class TrojanLimitFlowDto
    {
        /// <summary>
        /// 用户
        /// </summary>
        public int[] users { get; set; }
        /// <summary>
        /// 流量(-1为无限,单位为最小单位byte)
        /// </summary>
        public long quota { get; set; }
    }
}
