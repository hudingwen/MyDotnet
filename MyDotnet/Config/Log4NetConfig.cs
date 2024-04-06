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
            builder.Host.ConfigureLogging((context, loggingbuilder) =>
            {
                //过滤系统日志
                loggingbuilder.AddFilter("System", LogLevel.Information); //过滤掉系统默认的一些日志
                loggingbuilder.AddFilter("Microsoft", LogLevel.Information);//过滤掉系统默认的一些日志

                //添加Log4Net
                //var path = Directory.GetCurrentDirectory() + "\\log4net.config"; 
                //不带参数：表示log4net.config的配置文件就在应用程序根目录下，也可以指定配置文件的路径
                loggingbuilder.AddLog4Net();
            });
        }
    }
}
