﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDotnet.Domain.Dto.System
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    public class MutiDBOperate
    {
        /// <summary>
        /// 连接启用开关
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 连接ID
        /// </summary>
        public string ConnId { get; set; }

        /// <summary>
        /// 从库执行级别，越大越先执行
        /// </summary>
        public int HitRate { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string Connection { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DataBaseType DbType { get; set; }
    }
}
