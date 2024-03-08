

using Microsoft.Extensions.DependencyInjection;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.System;

namespace MyDotnet.Tasks.HostJob
{
    /// <summary>
    /// 调度任务初始化
    /// </summary>
    public class InitDatabaseJobHostedService : IHostedService
    { 
        public IServiceScopeFactory _services;

        public InitDatabaseJobHostedService(IServiceScopeFactory services)
        { 
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
                if(EnableConfig.initDataBase)
                {
                    using (var scope = _services.CreateScope())
                    {
                        CodeFirstService codeFirstService = scope.ServiceProvider.GetRequiredService<CodeFirstService>();
                        await codeFirstService.InitDatabase();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库初始化失败");
                LogHelper.logSys.Error("数据库初始化失败", ex);
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
