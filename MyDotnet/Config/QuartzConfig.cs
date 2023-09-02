using MyDotnet.Domain.Dto.System;
using Quartz;
using Quartz.Spi;
using System.Reflection;

namespace MyDotnet.Config
{
    /// <summary>
    /// 调度服务配置
    /// </summary>
    public static class QuartzConfig
    {
        /// <summary>
        /// 注入调度任务
        /// </summary>
        /// <param name="builder"></param>
        public static void SetQuartz(this WebApplicationBuilder builder)
        {
            //Quartz工厂
            builder.Services.AddSingleton<IJobFactory, JobFactory>();
            //Quartz管理注入
            builder.Services.AddSingleton(typeof(SchedulerCenterServer));

            //批量任务注入
            var baseType = typeof(IJob);
            var types = Assembly.GetEntryAssembly()
                .GetExportedTypes()
                .Where(x => x != baseType && baseType.IsAssignableFrom(x) && x.IsClass).ToArray();
            foreach (var implementType in types)
            {
                builder.Services.AddTransient(implementType);
            }
        }

    }
}
