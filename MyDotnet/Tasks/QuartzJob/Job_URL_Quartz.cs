
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using Quartz;

namespace MyDotnet.Tasks.QuartzJob
{
    public class Job_URL_Quartz : JobBase, IJob
    {
        public Job_URL_Quartz(BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices
            ) : base(tasksQzServices, tasksLogServices)
        {
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
                string pars = data.GetString("JobParam");
                if (!string.IsNullOrWhiteSpace(pars))
                {
                    var log = await HttpHelper.GetAsync(pars);
                }
            }
        }
    }



}
