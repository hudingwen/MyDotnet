﻿
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.WeChat;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.Ns;
using MyDotnet.Services.WeChat;
using Quartz;

namespace MyDotnet.Tasks.QuartzJob
{
    /// <summary>
    /// Nightscout定时停止实例
    /// </summary>
    public class Job_Nightscout_Stop_Quartz : JobBase, IJob
    {
        public Job_Nightscout_Stop_Quartz(BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices
            , BaseServices<NightscoutLog> nightscoutLogServices
            , NightscoutServices nightscoutServices
            , WeChatConfigServices weChatConfigServices
            , BaseServices<NightscoutServer> nightscoutServerServices
            ) : base(tasksQzServices, tasksLogServices)
        {
            _nightscoutLogServices = nightscoutLogServices;
            _nightscoutServices = nightscoutServices;
            _weChatConfigServices = weChatConfigServices;
            _nightscoutServerServices = nightscoutServerServices;
        }
        public BaseServices<NightscoutLog> _nightscoutLogServices { get; set; }
        public NightscoutServices _nightscoutServices { get; set; }
        public WeChatConfigServices _weChatConfigServices { get; set; }
        public BaseServices<NightscoutServer> _nightscoutServerServices { get; set; }


        public async Task Execute(IJobExecutionContext context)
        {
            // 可以直接获取 JobDetail 的值
            var jobKey = context.JobDetail.Key;
            var jobId = jobKey.Name;
            var executeLog = await ExecuteJob(context, async () => await Run(context, jobId.ObjToLong()));

        }
        public async Task Run(IJobExecutionContext context, long jobid)
        {
            if (jobid > 0)
            {
                var nights = await _nightscoutServices.Dal.Query();
                var servers = await _nightscoutServerServices.Dal.Query();
                nights = nights.Where(t => t.isStop == false).ToList();

                int i = 1;
                foreach (var server in servers)
                {
                    var serverNights = nights.FindAll(t => t.serverId == server.Id);
                    foreach (var nightscout in serverNights)
                    {
                        LogHelper.logApp.Info($"正在检索第{i}个,总计:{nights.Count}个");
                        await _nightscoutServices.StopLongTimeNoUseNs(nightscout, server);
                        i++;
                        Thread.Sleep(1000);
                    }
                    
                }
                
            }
        }
    }


}
