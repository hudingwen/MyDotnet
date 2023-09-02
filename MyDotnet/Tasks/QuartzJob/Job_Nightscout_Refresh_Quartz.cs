
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.WeChat;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.Ns;
using MyDotnet.Services.WeChat;
using Quartz;

namespace MyDotnet.Tasks.QuartzJob
{
    public class Job_Nightscout_Refresh_Quartz : JobBase, IJob
    {
        public Job_Nightscout_Refresh_Quartz(BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices
            , BaseServices<NightscoutLog> nightscoutLogServices
            , NightscoutServices nightscoutServices
            , WeChatConfigServices weChatConfigServices
            , BaseServices<NightscoutServer> nightscoutServerServices
            ) : base(tasksQzServices, tasksLogServices)
        {
            _nightscoutLogServices = nightscoutLogServices;
            _nightscoutServices = nightscoutServices;
            _weChatConfigServices = weChatConfigServices;
            _nightscoutServerServices = nightscoutServerServices;
        }
        public BaseServices<NightscoutLog> _nightscoutLogServices { get; set; }
        public NightscoutServices _nightscoutServices { get; set; }
        public WeChatConfigServices _weChatConfigServices { get; set; }
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
                JobDataMap data = context.JobDetail.JobDataMap;
                string pars = data.GetString("JobParam");
                var nsConfig = JsonHelper.JsonToObj<NightscoutRemindConfig>(pars);

                var nights = await _nightscoutServices.Dal.Query();
                List<string> errCount = new List<string>();
                foreach (var nightscout in nights)
                {
                    try
                    {
                        if (nightscout.isStop) continue;
                        var nsserver = await _nightscoutServerServices.Dal.QueryById(nightscout.serverId);
                        await _nightscoutServices.StopDocker(nightscout, nsserver);
                        await _nightscoutServices.RunDocker(nightscout, nsserver);
                    }
                    catch (Exception ex)
                    {
                        errCount.Add(nightscout.name);
                        LogHelper.logSys.Error($"{nightscout.name}-重启实例失败:{ex.Message}", ex);
                    }
                }
                if (errCount.Count > 0)
                {
                    try
                    {
                        var pushUsers = nsConfig.pushUserIDs.Split(",", StringSplitOptions.RemoveEmptyEntries);
                        if (pushUsers.Length > 0)
                        {
                            var pushWechatID = ConfigHelper.GetValue(new string[] { "nightscout", "pushWechatID" }).ObjToString();
                            var pushCompanyCode = ConfigHelper.GetValue(new string[] { "nightscout", "pushCompanyCode" }).ObjToString();
                            var pushTemplateID = ConfigHelper.GetValue(new string[] { "nightscout", "pushTemplateID_Alert" }).ObjToString();
                            var frontPage = ConfigHelper.GetValue(new string[] { "nightscout", "FrontPage" }).ObjToString();
                            foreach (var userid in pushUsers)
                            {
                                var pushData = new WeChatCardMsgDataDto();
                                pushData.cardMsg = new WeChatCardMsgDetailDto();
                                pushData.cardMsg.keyword1 = $"每周ns重启任务出现失败:{errCount.Count}个";
                                pushData.cardMsg.keyword2 = string.Join(",", errCount);
                                pushData.cardMsg.remark = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                pushData.cardMsg.url = frontPage;
                                pushData.cardMsg.template_id = pushTemplateID;
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
        }
    }


}
