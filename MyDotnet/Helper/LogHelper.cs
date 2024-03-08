using log4net;
using MyDotnet.Domain.Dto.System;

namespace MyDotnet.Helper
{
    /// <summary>
    /// 日志帮助类
    /// </summary>

    public static class LogHelper
    {
        /// <summary>
        /// 应用日志记录器
        /// </summary>
        public static readonly ILog logApp = LogManager.GetLogger(LogEnum.AppInfo.GetDisplayName());
        /// <summary>
        /// 请求日志记录器
        /// </summary>
        public static readonly ILog logNet = LogManager.GetLogger(LogEnum.RequestInfo.GetDisplayName());
        /// <summary>
        /// 全局日志记录器
        /// </summary>
        public static readonly ILog logSys = LogManager.GetLogger(LogEnum.GlobalInfo.GetDisplayName());
        /// <summary>
        /// sql日志记录器
        /// </summary>
        public static readonly ILog logSql = LogManager.GetLogger(LogEnum.SqlInfo.GetDisplayName());
    }
}
