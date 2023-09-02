
using MyDotnet.Domain.Entity.System;
using MyDotnet.Domain.Entity.Trojan;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using Quartz;

namespace MyDotnet.Tasks.QuartzJob
{
    public class Job_Trojan_Quartz : JobBase, IJob
    {
        public Job_Trojan_Quartz(BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices
            , UnitOfWorkManage unitOfWorkManage
            , BaseServices<TrojanDetails> DetailServices
            , BaseServices<TrojanUsers> TrojanUsers
            ) : base(tasksQzServices, tasksLogServices)
        {
            _unitOfWorkManage = unitOfWorkManage;
            _DetailServices = DetailServices;
            _TrojanUsers = TrojanUsers;
        }
        public UnitOfWorkManage _unitOfWorkManage { get; set; }

        public BaseServices<TrojanDetails> _DetailServices { get; set; }

        public BaseServices<TrojanUsers> _TrojanUsers { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            //var param = context.MergedJobDataMap;
            // 可以直接获取 JobDetail 的值
            var jobKey = context.JobDetail.Key;
            var jobId = jobKey.Name;
            var executeLog = await ExecuteJob(context, async () => await Run(context, jobId.ObjToLong()));

        }
        public async Task Run(IJobExecutionContext context, long jobid)
        {
            if (jobid > 0)
            {
                try
                {
                    //获取每月用户的数据
                    _unitOfWorkManage.BeginTran();
                    var now = DateTime.Now.AddMonths(-1);

                    var list = await _TrojanUsers.Dal.Query();
                    List<TrojanDetails> ls = new List<TrojanDetails>();
                    foreach (var us in list)
                    {
                        TrojanDetails u = new TrojanDetails();
                        u.calDate = now;
                        u.userId = us.id;
                        u.download = us.download;
                        u.upload = us.upload;
                        //清零
                        us.download = 0;
                        us.upload = 0;
                        ls.Add(u);
                    }
                    await _TrojanUsers.Dal.Update(list);
                    await _DetailServices.Dal.Add(ls);
                    _unitOfWorkManage.CommitTran();
                }
                catch (Exception)
                {
                    _unitOfWorkManage.RollbackTran();
                    throw;
                }
            }
        }
    }



}
