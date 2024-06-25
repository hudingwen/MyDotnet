
using Amazon.Runtime.Internal.Util;
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
    [JobDescriptionAttribute("Nightscout监护任务", "定时获取血糖并上传")]
    public class Job_Nightscout_Guard_Quartz : JobBase, IJob
    {
        public NightscoutGuardService _nightscoutGuardService;
        private BaseRepository<NightscoutGuardAccount> _baseRepositoryAccount;
        public Job_Nightscout_Guard_Quartz(
            BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices  
            , NightscoutGuardService nightscoutGuardService
            , BaseRepository<NightscoutGuardAccount> baseRepositoryAccount
            ) : base(tasksQzServices, tasksLogServices)
        {
            _nightscoutGuardService = nightscoutGuardService;
            _baseRepositoryAccount = baseRepositoryAccount;
        } 


        public async Task Execute(IJobExecutionContext context)
        {
            // 可以直接获取 JobDetail 的值
            var jobKey = context.JobDetail.Key;
            var jobId = jobKey.Name;
            var executeLog = await ExecuteJob(context, async () => await Run(context, jobId.ObjToLong()));
        }
        /// <summary>
        /// 审核中的设备
        /// </summary>
        public Dictionary<string,List<string>> processingDevices = new Dictionary<string, List<string>>();
        public async Task Run(IJobExecutionContext context, long jobid)
        {
            if (jobid > 0)
            {
                var users = await _nightscoutGuardService.getGuardUserList(1, 9999);
                foreach (var user in users.data)
                {
                    try
                    {
                        var accout = await _baseRepositoryAccount.QueryById(user.gid);

                        if(accout.guardType == 100)
                        {
                            //硅基
                            var data = await GuijiHelper.getGuijiUser(accout.token, user.uid);
                            if (data.success)
                            {
                                //推送数据
                                if(data.data.followedDeviceGlucoseDataPO.glucoseInfos != null && data.data.followedDeviceGlucoseDataPO.glucoseInfos.Count > 0)
                                {
                                    //正常
                                    var pushData = data.data.followedDeviceGlucoseDataPO.glucoseInfos.Where(t => t.time > user.refreshTime).OrderBy(t => t.time).ToList();
                                    if (pushData.Count > 0)
                                    {
                                        //推送
                                        var send = pushData.Select(t => new NsUploadBloodInfo { date = t.t ,sgv = t.v * 18, direction = _nightscoutGuardService.GetNsFlagForGuiji(t.s) }).OrderBy(t=>t.date).ToList();
                                        await _nightscoutGuardService.pushBlood(user, send);
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
                                        send.Add(new NsUploadBloodInfo() { date = data.data.followedDeviceGlucoseDataPO.latestGlucoseTime, sgv = data.data.followedDeviceGlucoseDataPO.latestGlucoseValue * 18 , direction = _nightscoutGuardService.GetNsFlagForGuiji(data.data.followedDeviceGlucoseDataPO.bloodGlucoseTrend) });
                                        await _nightscoutGuardService.pushBlood(user, send);
                                    }

                                }
                            }
                            else
                            {
                                LogHelper.logApp.Error($"监护用户:{user.name} 监护异常:{data.msg}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.logApp.Error($"监护用户:{user.name} 操作异常:{ex.Message}", ex); 
                    }

                }
            }
        }
    }


}
