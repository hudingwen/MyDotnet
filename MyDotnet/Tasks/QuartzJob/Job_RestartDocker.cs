
using MyDotnet.Domain.Attr;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using Quartz;
using Renci.SshNet;

namespace MyDotnet.Tasks.QuartzJob
{
    /// <summary>
    /// 定时重启Nginx服务
    /// </summary>
    [JobDescriptionAttribute("定时重启Nginx服务", "查找服务器为Nginx的标记进行重启")]
    public class Job_RestartDocker : JobBase, IJob
    {
        public Job_RestartDocker(BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices
            , BaseServices<NightscoutServer> nightscoutServerServices
            ) : base(tasksQzServices, tasksLogServices)
        {
            _nightscoutServerServices = nightscoutServerServices;
        }

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
                //重启docker服务
                var master = (await _nightscoutServerServices.Dal.Query(t => t.isNginx == true)).FirstOrDefault();
                if (master != null)
                {
                    using (var sshMasterClient = new SshClient(master.serverIp, master.serverPort, master.serverLoginName, master.serverLoginPassword))
                    {
                        sshMasterClient.Connect();
                        using (var cmdMaster = sshMasterClient.CreateCommand(""))
                        {
                            JobDataMap data = context.JobDetail.JobDataMap;
                            string pars = data.GetString("JobParam");
                            var resMaster = cmdMaster.Execute(pars);
                        }
                        sshMasterClient.Disconnect();
                    }
                }
            }
        }
    }



}
