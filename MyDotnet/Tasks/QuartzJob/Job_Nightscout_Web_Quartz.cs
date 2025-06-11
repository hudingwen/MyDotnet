using MyDotnet.Domain.Attr;
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
    /// Nightscout定时切换CDN
    /// </summary>
    [JobDescriptionAttribute("NightscoutWeb检测", "出现异常时自动发出告警")]
    public class Job_Nightscout_Web_Quartz : JobBase, IJob
    {
        public Job_Nightscout_Web_Quartz(BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices
            , BaseServices<NightscoutLog> nightscoutLogServices
            , NightscoutServices nightscoutServices
            , WeChatConfigServices weChatConfigServices
            , BaseServices<NightscoutServer> nightscoutServerServices
            , DicService dicService
            ) : base(tasksQzServices, tasksLogServices)
        {
            _nightscoutLogServices = nightscoutLogServices;
            _nightscoutServices = nightscoutServices;
            _weChatConfigServices = weChatConfigServices;
            _nightscoutServerServices = nightscoutServerServices;
            _dicService = dicService;
        }
        public DicService _dicService;
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


                //web检测错误次数提醒
                var webErrorCount = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.webErrorCount);
                //cdn间隔时间
                var webErrorSleep = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.webErrorSleep);


                //连续多少次错误切换
                int errTotal = webErrorCount.content.ObjToInt();
                //间隔
                int sleepTime = webErrorSleep.content.ObjToInt();

                //是否结束任务 1-运行 2-退出
                string existStatus = "1";

                //当前错误次数
                Dictionary<string, int> errInfo = new Dictionary<string, int>();

                while (true && "1".Equals(existStatus))
                {

                    //web列表
                    var webList = await _dicService.GetDicData(WebList.KEY);
                    foreach (var web in webList)
                    {
                        bool isSuccess = false;
                        try
                        {
                            var data = await HttpHelper.GetAsyncResponse(web.content);
                            isSuccess = data.IsSuccessStatusCode;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.logSys.Error($"访问失败,{ex.Message}", ex);
                        }

                        if (!isSuccess)
                        {
                            if (errInfo.ContainsKey(web.name))
                            {
                                errInfo[web.name] = errInfo[web.name] + 1;
                            }
                            else
                            {
                                errInfo.Add(web.name, 0);
                            }
                        }
                        else
                        {
                            if (errInfo.ContainsKey(web.name))
                            {
                                errInfo[web.name] = 0;
                            }
                            else
                            {
                                errInfo.Add(web.name, 0);
                            }
                        }

                    }


                    foreach (var err in errInfo)
                    {
                        try
                        {
                            if(err.Value>= errTotal)
                            {
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
                                        pushData.cardMsg.keyword1 = $"发生时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
                                        pushData.cardMsg.keyword2 = $"网站无法访问:{err.Key}";
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
                            
                        }
                        catch (Exception ex)
                        {
                            LogHelper.logSys.Error($"推送失败,{ex.Message}", ex);
                        }
                    }
                   


                    //间隔
                    Thread.Sleep(sleepTime * 1000);
                    //退出标识
                    var cdnCheckFinish = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.cdnCheckFinish);
                    existStatus = cdnCheckFinish.content;
                }

            }
        }
    }



}
