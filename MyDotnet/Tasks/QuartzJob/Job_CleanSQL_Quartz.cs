
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using Quartz;

namespace MyDotnet.Tasks.QuartzJob
{
    public class Job_CleanSQL_Quartz : JobBase, IJob
    {
        public Job_CleanSQL_Quartz(BaseServices<TasksQz> tasksQzServices
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
                pars = string.Format(pars, DateTime.Now.ToString("yyyyMM01"));
                await _tasksQzServices.Dal.Db.Ado.ExecuteCommandAsync(pars);
            }
        }
    }



}
