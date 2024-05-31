
using MyDotnet.Domain.Attr;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.WeChat;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.Ns;
using MyDotnet.Services.System;
using MyDotnet.Services.WeChat;
using Quartz;
using Renci.SshNet;
using System.Text;

namespace MyDotnet.Tasks.QuartzJob
{
    /// <summary>
    /// Nightscout定时刷新
    /// </summary>
    [JobDescriptionAttribute("Nightscout定时刷新", "执行此任务会把每个服务器的实例都重启一次")]
    public class Job_Nightscout_Refresh_Quartz : JobBase, IJob
    {
        public Job_Nightscout_Refresh_Quartz(BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices
            , BaseServices<NightscoutLog> nightscoutLogServices
            , NightscoutServices nightscoutServices
            , WeChatConfigServices weChatConfigServices
            , BaseServices<NightscoutServer> nightscoutServerServices
            , DicService dicService
            ) : base(tasksQzServices, tasksLogServices)
        {
            _nightscoutLogServices = nightscoutLogServices;
            _nightscoutServices = nightscoutServices;
            _weChatConfigServices = weChatConfigServices;
            _nightscoutServerServices = nightscoutServerServices;
            _dicService = dicService;
        }
        public BaseServices<NightscoutLog> _nightscoutLogServices { get; set; }
        public NightscoutServices _nightscoutServices { get; set; }
        public WeChatConfigServices _weChatConfigServices { get; set; }
        public BaseServices<NightscoutServer> _nightscoutServerServices { get; set; }
        public DicService _dicService { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            // 可以直接获取 JobDetail 的值
            var jobKey = context.JobDetail.Key;
            var jobId = jobKey.Name;
            var executeLog = await ExecuteJob(context, async () => await Run(context, jobId.ObjToLong()));

        }
        public async Task Run(IJobExecutionContext context, long jobid)
        {
            if (jobid <= 0)
                return;

            var servers = await _nightscoutServerServices.Dal.Query();
             
            foreach ( var server in servers )
            {
                await _nightscoutServices.RestartServer(server.Id.ObjToString());
            }



        }
    }


}
