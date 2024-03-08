

using Microsoft.Extensions.DependencyInjection;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;

namespace MyDotnet.Tasks.HostJob
{
    /// <summary>
    /// 调度任务初始化
    /// </summary>
    public class QuartzJobHostedService : IHostedService
    {
        public SchedulerCenterServer _schedulerCenterServer;
        public IServiceScopeFactory _services;

        public QuartzJobHostedService(
            SchedulerCenterServer schedulerCenter,
            IServiceScopeFactory services
            )
        {
            _schedulerCenterServer = schedulerCenter;
            _services = services;
        }
        /// <summary>
        /// 开始任务
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if(EnableConfig.quartzEnable)
                {
                    using (var scope = _services.CreateScope())
                    {
                        BaseServices<TasksQz> _tasksQzServices = scope.ServiceProvider.GetRequiredService<BaseServices<TasksQz>>();
                        var allQzServices = await _tasksQzServices.Dal.Query();
                        foreach (var item in allQzServices)
                        {
                            if (item.IsStart)
                            {
                                var result = await _schedulerCenterServer.AddScheduleJobAsync(item);
                                if (result.success)
                                {
                                    Console.WriteLine($"QuartzNetJob{item.Name}启动成功！");
                                }
                                else
                                {
                                    Console.WriteLine($"QuartzNetJob{item.Name}启动失败！错误信息：{result.msg}");
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"调度服务启动失败");
                LogHelper.logSys.Error("调度服务启动失败",ex);
            }
        }
        /// <summary>
        /// 结束任务
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
