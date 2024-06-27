﻿
using Amazon.Runtime.Internal.Util;
using AppStoreConnect.Model;
using MyDotnet.Domain.Attr;
using MyDotnet.Domain.Dto.Apple;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Dto.WeChat;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using MyDotnet.Services.Ns;
using MyDotnet.Services.System;
using MyDotnet.Services.WeChat;
using Quartz;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace MyDotnet.Tasks.QuartzJob
{
    /// <summary>
    /// Nightscout监护任务
    /// </summary>
    [JobDescriptionAttribute("Nightscout监护账户任务", "定时监控监护账户有效性")]
    public class Job_Nightscout_Guard_Quartz : JobBase, IJob
    {
        public NightscoutGuardService _nightscoutGuardService;
        public BaseRepository<NightscoutGuardAccount> _baseRepositoryAccount;
        public SchedulerCenterServer _schedulerCenter; 
        public Job_Nightscout_Guard_Quartz(
            BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices  
            , NightscoutGuardService nightscoutGuardService
            , BaseRepository<NightscoutGuardAccount> baseRepositoryAccount
            , SchedulerCenterServer schedulerCenter 
            ) : base(tasksQzServices, tasksLogServices)
        {
            _nightscoutGuardService = nightscoutGuardService;
            _baseRepositoryAccount = baseRepositoryAccount;
            _schedulerCenter = schedulerCenter; 
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
                //获取监护账户
                var accounts = await _baseRepositoryAccount.Query();
                foreach ( var account in accounts)
                {
                    try
                    {

                        //判断账户是否失效
                        await _nightscoutGuardService.CheckAccount(account);

                        //获取监护用户
                        var users = await _nightscoutGuardService.Dal.Query(t=>t.gid == account.Id & t.Enabled == true);

                        foreach (var user in users)
                        {
                            //开启下一次获取血糖任务

                            //查找任务(监护用户id作为任务id,一个用户只有一个任务)
                            var userTask = (await _tasksQzServices.Dal.Query(t => t.ResourceId == user.Id)).FirstOrDefault();
                            if(userTask == null)
                            {
                                userTask = new TasksQz();
                                //创建任务信息
                                var taskInfo = typeof(Job_Nightscout_Guard_User_Quartz);
                                userTask.AssemblyName = taskInfo.Namespace;
                                userTask.ClassName = taskInfo.Name;
                                //"50 1 9 30 6 ? 2024"
                                //10秒后执行任务
                                userTask.Cron = DateTime.Now.AddSeconds(30).ToString("ss mm HH dd MM ? yyyy");
                                userTask.CycleHasRunTimes = 0;
                                userTask.CycleRunTimes = 1;
                                userTask.Enabled = true;
                                userTask.BeginTime = user.startTime;
                                userTask.EndTime = user.endTime;
                                userTask.IntervalSecond = 1;
                                userTask.JobGroup = "监护用户任务";
                                userTask.Name = user.name;
                                userTask.TriggerType = 1;
                                userTask.IsStart = true;
                                userTask.ResourceId = user.Id;
                                //添加任务
                                await _tasksQzServices.Dal.Add(userTask);
                                await _schedulerCenter.AddScheduleJobAsync(userTask);
                            }
                            else
                            {
                                var isRunnig = await _schedulerCenter.IsExistScheduleJobAsync(userTask);
                                if (!isRunnig)
                                {
                                    //主动运行
                                    userTask.Cron = DateTime.Now.AddSeconds(30).ToString("ss mm HH dd MM ? yyyy");
                                    userTask.Name = user.name;
                                    userTask.BeginTime = user.startTime;
                                    userTask.EndTime = user.endTime;
                                    await _tasksQzServices.Dal.Update(userTask);
                                    await _schedulerCenter.AddScheduleJobAsync(userTask);
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.logApp.Error($"监护账户异常:{account.name} ", ex);
                    }
                }


                
            }
        }
    }


}
