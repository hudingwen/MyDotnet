
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.WeChat;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.Ns;
using MyDotnet.Services.System;
using MyDotnet.Services.WeChat;
using Quartz;

namespace MyDotnet.Tasks.QuartzJob
{
    /// <summary>
    /// Nightscout到期提醒
    /// </summary>
    public class Job_Nightscout_Quartz : JobBase, IJob
    {
        public Job_Nightscout_Quartz(BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices
            , BaseServices<NightscoutLog> nightscoutLogServices
            , NightscoutServices nightscoutServices
            , WeChatConfigServices weChatConfigServices
            , DicService dicService
            ) : base(tasksQzServices, tasksLogServices)
        {
            _nightscoutLogServices = nightscoutLogServices;
            _nightscoutServices = nightscoutServices;
            _weChatConfigServices = weChatConfigServices;
            _dicService = dicService;
        }

        public BaseServices<NightscoutLog> _nightscoutLogServices { get; set; }
        public NightscoutServices _nightscoutServices { get; set; }
        public WeChatConfigServices _weChatConfigServices { get; set; }

        public DicService _dicService { get; set; }

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
                //JobDataMap data = context.JobDetail.JobDataMap;
                //string pars = data.GetString("JobParam");


                var nights = await _nightscoutServices.Dal.Query();

                var nsInfo = await _dicService.GetDicData(NsInfo.KEY);
                var frontPage = nsInfo.Find(t => t.code.Equals(NsInfo.frontPage)).content;
                var pushTemplateID_Alert = nsInfo.Find(t => t.code.Equals(NsInfo.pushTemplateID_Alert)).content;
                var pushWechatID = nsInfo.Find(t => t.code.Equals(NsInfo.pushWechatID)).content;
                var pushCompanyCode = nsInfo.Find(t => t.code.Equals(NsInfo.pushCompanyCode)).content;

                var preDayInfo = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.preDays);
                var preDay = preDayInfo.content.ObjToInt();

                var preInnerUser = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.preInnerUser);
                
                

                List<string> ls = new List<string>();
                foreach (var nightscout in nights)
                {
                    try
                    {
                        WeChatCardMsgDataDto pushData = null;
                        if (DateTime.Now.Date.AddDays(preDay) >= nightscout.endTime)
                        {
                            ls.Add(nightscout.name);
                            pushData = new WeChatCardMsgDataDto();
                            pushData.cardMsg = new WeChatCardMsgDetailDto();
                            pushData.cardMsg.first = $"{nightscout.name},你的ns服务即将到期";

                        }
                        else if (DateTime.Now.Date >= nightscout.endTime)
                        {
                            ls.Add(nightscout.name);
                            pushData = new WeChatCardMsgDataDto();
                            pushData.cardMsg = new WeChatCardMsgDetailDto();
                            pushData.cardMsg.first = $"{nightscout.name},你的ns服务已到期";

                        }

                        if (pushData != null)
                        {
                            pushData.cardMsg.keyword1 = $"NS服务即将到期,请尽快续费,以免中断服务";
                            pushData.cardMsg.keyword2 = $"{nightscout.endTime.ToString("yyyy-MM-dd")}";
                            pushData.cardMsg.remark = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            pushData.cardMsg.url = $"https://{nightscout.url}";
                            pushData.cardMsg.template_id = pushTemplateID_Alert;
                            pushData.info = new WeChatUserInfo();
                            pushData.info.id = pushWechatID;
                            pushData.info.companyCode = pushCompanyCode;
                            pushData.info.userID = nightscout.Id.ToString();
                            await _weChatConfigServices.PushCardMsg(pushData);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.logSys.Error($"{nightscout.name}推送失败,{ex.Message}", ex);
                    }
                }
                if (ls.Count > 0)
                {
                    try
                    {
                        var pushUsers = preInnerUser.content.Split(",", StringSplitOptions.RemoveEmptyEntries);
                        if (pushUsers.Length > 0)
                        {
                            
                            foreach (var userid in pushUsers)
                            {
                                var pushData = new WeChatCardMsgDataDto();
                                pushData.cardMsg = new WeChatCardMsgDetailDto();
                                pushData.cardMsg.keyword1 = $"有{ls.Count}个客户即将到期或已到期";
                                pushData.cardMsg.keyword2 = string.Join(",", ls);
                                pushData.cardMsg.remark = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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
        }
    }
}
