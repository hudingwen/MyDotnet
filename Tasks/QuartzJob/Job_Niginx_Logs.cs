
using MyDotnet.Domain.Attr;
using MyDotnet.Domain.Entity.Nginx;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using MyDotnet.Services.Analyze;
using Quartz;

namespace MyDotnet.Tasks.QuartzJob
{
    /// <summary>
    /// Nginx日志分析任务
    /// </summary>
    [JobDescriptionAttribute("Nginx日志分析任务", "分析一个日志文件,任务参数为文件路径")]
    public class Job_Niginx_Logs : JobBase, IJob
    {
        private NginxLogService _nginxLogService;

        private BaseRepository<NginxHostRequest> _baseRepositoryHost;
        private BaseRepository<NginxHostUrlRequest> _baseRepositoryUrl;
        public Job_Niginx_Logs(BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices
            , NginxLogService nginxLogService
            , BaseRepository<NginxHostRequest> baseRepositoryHost
            , BaseRepository<NginxHostUrlRequest> baseRepositoryUrl
            ) : base(tasksQzServices, tasksLogServices)
        {
            _nginxLogService = nginxLogService;
            _baseRepositoryHost = baseRepositoryHost;
            _baseRepositoryUrl = baseRepositoryUrl;
        }

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
                JobDataMap data = context.JobDetail.JobDataMap;
                string path = data.GetString("JobParam");
                var list = await _nginxLogService.AnalyzeLog(path);
                await _baseRepositoryHost.Delete(t => t.Id > 0);
                await _baseRepositoryHost.Add(list);
            }
        }
    }



}
