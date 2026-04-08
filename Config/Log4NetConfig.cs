using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyDotnet.Config
{
    /// <summary>
    /// 日志配置
    /// </summary>
    public static class Log4NetConfig
    {
        /// <summary>
        /// 注册日志
        /// </summary>
        /// <param name="builder"></param>
        public static void SetLog4Net(this WebApplicationBuilder builder)
        {
            // 日志配置
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            // 或指定路径：AddLog4Net("log4net.config")
            // 添加Log4Net
            // var path = Directory.GetCurrentDirectory() + "\\log4net.config"; 
            // 不带参数：表示log4net.config的配置文件就在应用程序根目录下，也可以指定配置文件的路径
            builder.Logging.AddLog4Net();
            //筛选日志
            builder.Logging.AddFilter("System", LogLevel.Information);
            builder.Logging.AddFilter("Microsoft", LogLevel.Information);
            builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
            builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
            builder.Logging.AddFilter("Quartz", LogLevel.Warning);


        }
    }
}
