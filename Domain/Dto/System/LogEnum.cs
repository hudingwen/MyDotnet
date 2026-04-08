using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDotnet.Domain.Dto.System
{
    /// <summary>
    /// 日记录器类型
    /// </summary>
    public enum LogEnum
    {
        /// <summary>
        /// 全局异常日志
        /// </summary>
        GlobalInfo,
        /// <summary>
        /// 网络请求日志
        /// </summary>
        RequestInfo,
        /// <summary>
        /// 应用程序日志
        /// </summary>
        AppInfo,
        /// <summary>
        /// sql日志
        /// </summary>
        SqlInfo
    }
}
