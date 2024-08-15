
using Amazon.Runtime.Internal.Util;
using AppStoreConnect.Model;
using MyDotnet.Domain.Attr;
using MyDotnet.Domain.Dto.Apple;
using MyDotnet.Domain.Dto.ExceptionDomain;
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
using System;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace MyDotnet.Tasks.QuartzJob
{
    /// <summary>
    /// Nightscout监护任务
    /// </summary>
    [JobDescriptionAttribute("Nightscout监护用户任务", "定时获取血糖并上传")]
    public class Job_Nightscout_Guard_User_Quartz : JobBase, IJob
    {
        public NightscoutGuardService _nightscoutGuardService;
        public BaseRepository<NightscoutGuardAccount> _baseRepositoryAccount;
        public SchedulerCenterServer _schedulerCenter; 
        public Job_Nightscout_Guard_User_Quartz(
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

                var task = await _tasksQzServices.Dal.QueryById(jobid);

                //获取监护用户
                var user = await _nightscoutGuardService.Dal.QueryById(task.ResourceId);
                if (user == null) throw new ServiceException($"监护用户获取失败:{jobid}");
                if(!user.Enabled)
                {
                    error = "用户未启用,任务结束";
                    return;
                }
                var account = await _baseRepositoryAccount.QueryById(user.gid);
                if (account == null) throw new ServiceException($"监护账号获取失败:{user.gid}");
                try
                {
                    if ("100".Equals(account.guardType))
                    {
                        //硅基
                        #region 硅基
                        var data = await GuijiHelper.getUserBlood(account.token, user.uid);
                        if (data.success)
                        {
                            bool isTouchTime = false;
                            var nextTime = DateTime.Now;
                            //推送数据
                            if (data.data.followedDeviceGlucoseDataPO.glucoseInfos != null && data.data.followedDeviceGlucoseDataPO.glucoseInfos.Count > 0 && data.data.followedDeviceGlucoseDataPO.time == data.data.followedDeviceGlucoseDataPO.glucoseInfos[0].time)
                            {
                                //正常
                                var pushData = data.data.followedDeviceGlucoseDataPO.glucoseInfos.Where(t => t.time > user.refreshTime).OrderBy(t => t.time).ToList();
                                if (pushData.Count > 0)
                                {
                                    //推送
                                    var send = pushData.Select(t => new NsUploadBloodInfo { date = t.t, sgv = t.v * 18, direction = _nightscoutGuardService.GetNsFlagForGuiji(t.s) }).OrderBy(t => t.date).ToList();
                                    await _nightscoutGuardService.pushBlood(user, send);
                                    nextTime = pushData[pushData.Count - 1].time.AddMinutes(5).AddSeconds(2);
                                    isTouchTime = true;
                                }
                            }
                            else
                            {
                                //延期
                                if (data.data.followedDeviceGlucoseDataPO.time > user.refreshTime)
                                {
                                    //大于上次更新才更新
                                    //推送
                                    var send = new List<NsUploadBloodInfo>();
                                    send.Add(new NsUploadBloodInfo() { date = data.data.followedDeviceGlucoseDataPO.latestGlucoseTime, sgv = data.data.followedDeviceGlucoseDataPO.latestGlucoseValue * 18, direction = _nightscoutGuardService.GetNsFlagForGuiji(data.data.followedDeviceGlucoseDataPO.bloodGlucoseTrend) });
                                    await _nightscoutGuardService.pushBlood(user, send);
                                    nextTime = DateTimeOffset.FromUnixTimeMilliseconds(send[send.Count - 1].date).UtcDateTime.ToLocalTime().AddMinutes(5).AddSeconds(2);
                                    isTouchTime = true;
                                }
                            }


                            // 更新cron表达式
                            var scheduler = context.Scheduler;
                            var jobKey = context.JobDetail.Key;

                            // 假设新的cron表达式是每分钟执行一次
                            string newCronExpression = string.Empty;
                            if (isTouchTime)
                            {
                                if(DateTime.Now >= nextTime)
                                {
                                    //调度时间异常处理
                                    nextTime = DateTime.Now.AddSeconds(30);
                                }
                                newCronExpression = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            }
                            else
                            {
                                nextTime = DateTime.Now.AddSeconds(30);
                                //默认30秒执行一次
                                newCronExpression = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            }

                            // 调用方法更新cron表达式
                            UpdateJobCronExpression(scheduler, jobKey, newCronExpression).Wait();

                            task.Cron = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            await _tasksQzServices.Dal.Update(task);
                        }
                        else
                        {
                            error = data.msg;
                            LogHelper.logApp.Error($"监护用户:{user.name} 监护异常:{data.msg}");
                        }
                        #endregion
                    }
                    else if ("200".Equals(account.guardType))
                    {
                        //三诺
                        #region 三诺
                        var data = await SannuoHelper.getUserBlood(account.token, user.uid);
                        if (data.success)
                        {
                            bool isTouchTime = false;
                            var nextTime = DateTime.Now;
                            

                            //趋势计算
                            _nightscoutGuardService.GetNsFlagForSannuo(data.data);

                            //推送数据
                            var pushData = data.data.Where(t => t.parsTime > user.refreshTime).OrderBy(t => t.time).ToList();
                            if (pushData.Count > 0)
                            {
                                //推送
                                var send = pushData.Select(t => new NsUploadBloodInfo { date = t.time, sgv = t.value * 18, direction = t.direction }).OrderBy(t => t.date).ToList();
                                await _nightscoutGuardService.pushBlood(user, send);
                                nextTime = pushData[pushData.Count - 1].parsTime.AddMinutes(3).AddSeconds(20);
                                isTouchTime = true;
                            }

                            // 更新cron表达式
                            var scheduler = context.Scheduler;
                            var jobKey = context.JobDetail.Key;

                            // 假设新的cron表达式是每分钟执行一次
                            string newCronExpression = string.Empty;
                            if (isTouchTime)
                            {
                                if (DateTime.Now >= nextTime)
                                {
                                    //调度时间异常处理
                                    nextTime = DateTime.Now.AddSeconds(30);
                                }
                                newCronExpression = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            }
                            else
                            {
                                nextTime = DateTime.Now.AddSeconds(30);
                                //默认30秒执行一次
                                newCronExpression = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            }

                            // 调用方法更新cron表达式
                            UpdateJobCronExpression(scheduler, jobKey, newCronExpression).Wait();

                            task.Cron = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            await _tasksQzServices.Dal.Update(task);
                        }
                        else
                        {
                            error = data.msg;
                            LogHelper.logApp.Error($"监护用户:{user.name} 监护异常:{data.msg}");
                        }
                        #endregion
                    }
                    else if ("300".Equals(account.guardType))
                    {
                        //微泰1
                        #region 微泰1
                        var data = await Weitai1Helper.getBlood(account.token, user.uid);
                        if ("100000".Equals(data.info?.code))
                        {
                            bool isTouchTime = false;
                            var nextTime = DateTime.Now;

                            var pushData = data.content.records.Where(t => t.eventType == 7 && t.deviceTime > user.refreshTime).OrderBy(t => t.deviceTime).ToList();
                            //趋势计算
                            _nightscoutGuardService.GetNsFlagForWeitai1(pushData);

                            //推送数据
                          
                            if (pushData.Count > 0)
                            {
                                //推送
                                var send = pushData.Select(t => new NsUploadBloodInfo { date = (new DateTimeOffset(t.deviceTime).ToUnixTimeMilliseconds()), sgv = t.eventData * 18, direction = t.direction }).OrderBy(t => t.date).ToList();
                                await _nightscoutGuardService.pushBlood(user, send);
                                nextTime = pushData[pushData.Count - 1].deviceTime.AddMinutes(5);
                                isTouchTime = true;
                            }

                            // 更新cron表达式
                            var scheduler = context.Scheduler;
                            var jobKey = context.JobDetail.Key;

                            // 假设新的cron表达式是每分钟执行一次
                            string newCronExpression = string.Empty;
                            if (isTouchTime)
                            {
                                if (DateTime.Now >= nextTime)
                                {
                                    //调度时间异常处理
                                    nextTime = DateTime.Now.AddSeconds(60);
                                }
                                newCronExpression = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            }
                            else
                            {
                                nextTime = DateTime.Now.AddSeconds(60);
                                //默认60秒执行一次
                                newCronExpression = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            }

                            // 调用方法更新cron表达式
                            UpdateJobCronExpression(scheduler, jobKey, newCronExpression).Wait();

                            task.Cron = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            await _tasksQzServices.Dal.Update(task);
                        }
                        else
                        {
                            error = data.info?.msg;
                            LogHelper.logApp.Error($"监护用户:{user.name} 监护异常:{data.info?.msg}");
                        }
                        #endregion
                    }
                    else if ("400".Equals(account.guardType))
                    {
                        //微泰2
                        #region 微泰2
                        var data = await Weitai2Helper.getBlood(account.token, user.uid);
                        if (data.code == 200)
                        {
                            bool isTouchTime = false;
                            var nextTime = DateTime.Now;

                            var pushData = data.data.Where(t => t.appCreateTime > user.refreshTime).OrderBy(t => t.appCreateTime).ToList();
                            //趋势计算
                            _nightscoutGuardService.GetNsFlagForWeitai2(pushData);

                            //推送数据

                            if (pushData.Count > 0)
                            {
                                //推送
                                var send = pushData.Select(t => new NsUploadBloodInfo { date = (new DateTimeOffset(t.appCreateTime).ToUnixTimeMilliseconds()), sgv = t.glucose, direction = t.direction }).OrderBy(t => t.date).ToList();
                                await _nightscoutGuardService.pushBlood(user, send);
                                nextTime = pushData[pushData.Count - 1].appCreateTime.AddMinutes(2);
                                isTouchTime = true;
                            }

                            // 更新cron表达式
                            var scheduler = context.Scheduler;
                            var jobKey = context.JobDetail.Key;

                            // 假设新的cron表达式是每分钟执行一次
                            string newCronExpression = string.Empty;
                            if (isTouchTime)
                            {
                                if (DateTime.Now >= nextTime)
                                {
                                    //调度时间异常处理
                                    nextTime = DateTime.Now.AddSeconds(30);
                                }
                                newCronExpression = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            }
                            else
                            {
                                nextTime = DateTime.Now.AddSeconds(30);
                                //默认60秒执行一次
                                newCronExpression = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            }

                            // 调用方法更新cron表达式
                            UpdateJobCronExpression(scheduler, jobKey, newCronExpression).Wait();

                            task.Cron = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            await _tasksQzServices.Dal.Update(task);
                        }
                        else
                        {
                            error = data.msg;
                            LogHelper.logApp.Error($"监护用户:{user.name} 监护异常:{data.msg}");
                        }
                        #endregion
                    }
                    else if ("500".Equals(account.guardType))
                    {
                        //欧泰
                        #region 欧泰
                        var data = await OutaiHelper.getBlood(account.token, account.loginName, user.uid);
                        if (data.state == 1)
                        {
                            bool isTouchTime = false;
                            var nextTime = DateTime.Now;

                            //趋势计算
                            _nightscoutGuardService.GetNsFlagForOutai(data.content.bloodSugarRecords.records);

                            var pushData = data.content.bloodSugarRecords.records.Where(t => t.timeFormat > user.refreshTime).OrderBy(t => t.timeFormat).ToList();
                            

                            //推送数据

                            if (pushData.Count > 0)
                            {
                                //推送
                                var send = pushData.Select(t => new NsUploadBloodInfo { date = (new DateTimeOffset(t.timeFormat).ToUnixTimeMilliseconds()), sgv = t.value * 18, direction = t.direction }).OrderBy(t => t.date).ToList();
                                await _nightscoutGuardService.pushBlood(user, send);
                                nextTime = pushData[pushData.Count - 1].timeFormat.AddMinutes(5);
                                isTouchTime = true;
                            }

                            // 更新cron表达式
                            var scheduler = context.Scheduler;
                            var jobKey = context.JobDetail.Key;

                            // 假设新的cron表达式是每分钟执行一次
                            string newCronExpression = string.Empty;
                            if (isTouchTime)
                            {
                                if (DateTime.Now >= nextTime)
                                {
                                    //调度时间异常处理
                                    nextTime = DateTime.Now.AddSeconds(30);
                                }
                                newCronExpression = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            }
                            else
                            {
                                nextTime = DateTime.Now.AddSeconds(30);
                                //默认60秒执行一次
                                newCronExpression = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            }

                            // 调用方法更新cron表达式
                            UpdateJobCronExpression(scheduler, jobKey, newCronExpression).Wait();

                            task.Cron = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            await _tasksQzServices.Dal.Update(task);
                        }
                        else
                        {
                            error = data.msg;
                            LogHelper.logApp.Error($"监护用户:{user.name} 监护异常:{data.msg}");
                        }
                        #endregion
                    }else if ("110".Equals(account.guardType))
                    {
                        //硅基轻享
                        #region 硅基轻享
                        var data = await GuijiLiteHelper.getUserBlood(account.token, user.uid);
                        if (data.success)
                        {
                            bool isTouchTime = false;
                            var nextTime = DateTime.Now;
                            //推送数据
                            if (data.data.followedDeviceGlucoseDataPO.glucoseInfos != null && data.data.followedDeviceGlucoseDataPO.glucoseInfos.Count > 0 && data.data.followedDeviceGlucoseDataPO.time == data.data.followedDeviceGlucoseDataPO.glucoseInfos[0].time)
                            {
                                //正常
                                var pushData = data.data.followedDeviceGlucoseDataPO.glucoseInfos.Where(t => t.time > user.refreshTime).OrderBy(t => t.time).ToList();
                                if (pushData.Count > 0)
                                {
                                    //推送
                                    var send = pushData.Select(t => new NsUploadBloodInfo { date = t.t, sgv = t.v * 18, direction = _nightscoutGuardService.GetNsFlagForGuiji(t.s) }).OrderBy(t => t.date).ToList();
                                    await _nightscoutGuardService.pushBlood(user, send);
                                    nextTime = pushData[pushData.Count - 1].time.AddMinutes(5).AddSeconds(2);
                                    isTouchTime = true;
                                }
                            }
                            else
                            {
                                //延期
                                if (data.data.followedDeviceGlucoseDataPO.time > user.refreshTime)
                                {
                                    //大于上次更新才更新
                                    //推送
                                    var send = new List<NsUploadBloodInfo>();
                                    send.Add(new NsUploadBloodInfo() { date = data.data.followedDeviceGlucoseDataPO.latestGlucoseTime, sgv = data.data.followedDeviceGlucoseDataPO.latestGlucoseValue * 18, direction = _nightscoutGuardService.GetNsFlagForGuiji(data.data.followedDeviceGlucoseDataPO.bloodGlucoseTrend) });
                                    await _nightscoutGuardService.pushBlood(user, send);
                                    nextTime = DateTimeOffset.FromUnixTimeMilliseconds(send[send.Count - 1].date).UtcDateTime.ToLocalTime().AddMinutes(5).AddSeconds(2);
                                    isTouchTime = true;
                                }
                            }


                            // 更新cron表达式
                            var scheduler = context.Scheduler;
                            var jobKey = context.JobDetail.Key;

                            // 假设新的cron表达式是每分钟执行一次
                            string newCronExpression = string.Empty;
                            if (isTouchTime)
                            {
                                if (DateTime.Now >= nextTime)
                                {
                                    //调度时间异常处理
                                    nextTime = DateTime.Now.AddSeconds(30);
                                }
                                newCronExpression = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            }
                            else
                            {
                                nextTime = DateTime.Now.AddSeconds(30);
                                //默认30秒执行一次
                                newCronExpression = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            }

                            // 调用方法更新cron表达式
                            UpdateJobCronExpression(scheduler, jobKey, newCronExpression).Wait();

                            task.Cron = nextTime.ToString("ss mm HH dd MM ? yyyy");
                            await _tasksQzServices.Dal.Update(task);
                        }
                        else
                        {
                            error = data.msg;
                            LogHelper.logApp.Error($"监护用户:{user.name} 监护异常:{data.msg}");
                        }
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    LogHelper.logApp.Error($"监护用户:{user.name} 操作异常:{ex.Message}", ex);
                } 
            }
        }

        private async Task UpdateJobCronExpression(IScheduler scheduler, JobKey jobKey, string newCronExpression)
        {
            var triggers = await scheduler.GetTriggersOfJob(jobKey);
            foreach (var trigger in triggers)
            {
                if (trigger is ICronTrigger cronTrigger)
                {
                    var newTrigger = TriggerBuilder.Create()
                        .WithIdentity(trigger.Key)
                        .WithCronSchedule(newCronExpression)
                        .ForJob(jobKey)
                        .Build();

                    await scheduler.RescheduleJob(trigger.Key, newTrigger);
                    Console.WriteLine($"Updated cron expression to: {newCronExpression}");
                }
            }
        }
    }


}
