
using MyDotnet.Tasks.HostJob;

namespace MyDotnet.Config
{
    /// <summary>
    /// 初始化任务配置(适用web服务开启后执行的任务)
    /// </summary>
    public static class HostJobConfig
    {
        /// <summary>
        /// 设置应用初始化任务
        /// </summary>
        /// <param name="builder"></param>
        public static void SetHostJob(this WebApplicationBuilder builder)
        {
            builder.Services.AddHostedService<InitDatabaseJobHostedService>();
            builder.Services.AddHostedService<QuartzJobHostedService>();

        }
    }
}
