
using Amazon.Runtime.Internal.Util;
using AppStoreConnect.Model;
using MyDotnet.Config;
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
using static Quartz.Logging.OperationName;

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
        public DicService _dicService;
        public WeChatConfigServices _weChatConfigServices { get; set; }
        public Job_Nightscout_Guard_Quartz(
            BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices  
            , NightscoutGuardService nightscoutGuardService
            , BaseRepository<NightscoutGuardAccount> baseRepositoryAccount
            , SchedulerCenterServer schedulerCenter 
            , DicService dicService
            , WeChatConfigServices weChatConfigServices
            ) : base(tasksQzServices, tasksLogServices)
        {
            _nightscoutGuardService = nightscoutGuardService;
            _baseRepositoryAccount = baseRepositoryAccount;
            _schedulerCenter = schedulerCenter;
            _dicService = dicService;
            _weChatConfigServices = weChatConfigServices;
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

                var accountTypes = await _dicService.GetDicData(GuardInfoKey.GuardAccountTypeList);


                foreach ( var account in accounts)
                {
                    try
                    {

                        //判断账户是否失效
                        var isOk = await _nightscoutGuardService.CheckAccount(account);
                        if (isOk)
                        {
                            //有效,判断前三天是否过期了
                            if((account.tokenExpire - DateTime.Now).TotalDays<3)
                            {
                                

                                try
                                {
                                    //刷新token 
                                    var isRefresh = await _nightscoutGuardService.refreshGuardAccount(account);
                                    var nsInfo = await _dicService.GetDicData(NsInfo.KEY);
                                    var frontPage = nsInfo.Find(t => t.code.Equals(NsInfo.frontPage)).content;
                                    var pushTemplateID_Alert = nsInfo.Find(t => t.code.Equals(NsInfo.pushTemplateID_Alert)).content;
                                    var pushWechatID = nsInfo.Find(t => t.code.Equals(NsInfo.pushWechatID)).content;
                                    var pushCompanyCode = nsInfo.Find(t => t.code.Equals(NsInfo.pushCompanyCode)).content;

                                    var preDayInfo = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.preDays);
                                    var preDay = preDayInfo.content.ObjToInt();

                                    var preInnerUser = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.preInnerUser);

                                    var pushUsers = preInnerUser.content.Split(",", StringSplitOptions.RemoveEmptyEntries);
                                    //恢复推送
                                    if (pushUsers.Length > 0)
                                    {
                                        foreach (var userid in pushUsers)
                                        {
                                            var pushData = new WeChatCardMsgDataDto();
                                            pushData.cardMsg = new WeChatCardMsgDetailDto();
                                            if (isRefresh.success)
                                            {
                                                pushData.cardMsg.keyword1 = $"{account.name} token 自动刷新成功";
                                            }
                                            else
                                            {
                                                pushData.cardMsg.keyword1 = $"{account.name} token 自动刷新失败,{isRefresh.msg}";
                                            }
                                            pushData.cardMsg.keyword2 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                            pushData.cardMsg.url = frontPage;
                                            pushData.cardMsg.template_id = pushTemplateID_Alert;
                                            pushData.info = new WeChatUserInfo();
                                            pushData.info.id = pushWechatID;
                                            pushData.info.companyCode = pushCompanyCode;
                                            pushData.info.userID = userid;
                                            await _weChatConfigServices.PushCardMsg(pushData);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.logSys.Error($"推送失败,{ex.Message}", ex);
                                }
                            }
                            
                        }
                        else
                        {
                            //失效后重新登录逻辑
                        }

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
                                var typeName = accountTypes.Find(t => t.code.Equals(account.guardType));
                                userTask.Name = $"{user.name}({user.nidUrl})|{typeName?.name}";
                                userTask.TriggerType = 1;
                                userTask.IsStart = true;
                                userTask.ResourceId = user.Id;
                                userTask.DistributeCode = QuartzConfig.guarCode;
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
                                    userTask.Name = $"{user.name}({user.nidUrl})";
                                    userTask.BeginTime = user.startTime;
                                    userTask.EndTime = user.endTime;
                                    await _tasksQzServices.Dal.Update(userTask);
                                    await _schedulerCenter.AddScheduleJobAsync(userTask);
                                }
                                else
                                { 
                                    TriggerKey triggerKey = new TriggerKey(userTask.Id.ToString(), userTask.JobGroup);
                                    TriggerState triggerState = await _schedulerCenter.GetTriggerState(triggerKey);

                                    //某些情况导致需要重新启动
                                    if (triggerState == TriggerState.Complete)
                                    {
                                        IJobDetail job = JobBuilder.Create<Job_Nightscout_Guard_User_Quartz>()
                                        .WithIdentity(userTask.Id.ToString(), userTask.JobGroup)
                                        .Build();
                                        // 创建一个新的触发器，新的cron表达式为每分钟执行一次
                                        ITrigger newTrigger = TriggerBuilder.Create()
                                            .WithIdentity(userTask.Id.ToString(), userTask.JobGroup)
                                            .WithCronSchedule(DateTime.Now.AddSeconds(30).ToString("ss mm HH dd MM ? yyyy"))
                                            .ForJob(job)
                                            .Build();

                                        // 重新调度任务
                                        await _schedulerCenter.RescheduleJob(triggerKey, newTrigger); 
                                    }
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
