using Microsoft.Extensions.DependencyInjection;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Services;
using Quartz;
using Quartz.Spi;
using System;

namespace MyDotnet.Domain.Dto.System
{
    /// <summary>
    /// quartz工厂
    /// </summary>
    public class JobFactory : IJobFactory
    {
        /// <summary>
        /// 注入反射获取依赖对象
        /// </summary>
        private readonly IServiceProvider _serviceProvider;
        public JobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// 实现接口Job
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {

                using (var scope = _serviceProvider.CreateScope())
                {
                    var data = scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType);
                    return data as IJob;
                } 
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ReturnJob(IJob job)
        {
            var disposable = job as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

        }
    }

}
