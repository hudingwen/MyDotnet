using MyDotnet.Domain.Attr;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Dto.WeChat;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.Ns;
using MyDotnet.Services.System;
using MyDotnet.Services.WeChat;
using Quartz;
using System.Net.NetworkInformation;

namespace MyDotnet.Tasks.QuartzJob
{
    /// <summary>
    /// Nightscout定时切换CDN
    /// </summary>
    [JobDescriptionAttribute("Nightscout网络检测", "出现异常时自动切换其他网络")]
    public class Job_Nightscout_CDN_Quartz : JobBase, IJob
    {
        public Job_Nightscout_CDN_Quartz(BaseServices<TasksQz> tasksQzServices
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
                //原始默认cdn
                var curCDN = await _dicService.GetDic(DicTypeList.defaultCDN);
                //cdn列表
                var cdnList = await _dicService.GetDicData(CDNList.KEY);
                //cdn检测错误次数提醒
                var cdnErrorCount = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.cdnErrorCount);
                //cdn间隔时间
                var cdnErrorSleep = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.cdnErrorSleep);

                Ping pingSender = new Ping();
                PingOptions options = new PingOptions
                {
                    // 不允许分片
                    DontFragment = true
                };
                int timeout = 5000; // 设置超时时间，单位为毫秒

                var curRow = cdnList.Find(t => t.code.Equals(curCDN.content));

                //连续多少次错误切换
                int errTotal = cdnErrorCount.content.ObjToInt();
                //间隔
                int sleepTime = cdnErrorSleep.content.ObjToInt();
                //当前错误次数
                int errCount = 0;
                //是否切换
                bool canSwitch = false;
                //是否进入异常时间
                bool isEnterErrTime = false;
                //记录进入异常时间
                DateTime enterErrTime = DateTime.Now;
                //是否结束任务 1-运行 2-退出
                string existStatus = "1";

                while (true && "1".Equals(existStatus))
                {
                    PingReply reply = await pingSender.SendPingAsync(curRow.content2, timeout, new byte[32], options);
                    if (reply.Status == IPStatus.Success)
                    {
                        //成功
                        errCount = 0;
                        if (canSwitch == true)
                        {
                            //网络正常切换回原来的
                            await _nightscoutServices.ChangeCDN(curRow.code);
                            try
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
                                        pushData.cardMsg.keyword1 = $"NS网络恢复,异常时间持续:{Math.Ceiling((DateTime.Now - enterErrTime).TotalMinutes)}分钟";
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

                            //恢复
                            canSwitch = false;
                            isEnterErrTime = false;
                        }
                        else
                        {
                            enterErrTime = DateTime.Now;
                        }
                    }
                    else
                    {
                        //失败
                        errCount += 1;
                        if(!isEnterErrTime) enterErrTime = DateTime.Now;
                        isEnterErrTime = true;
                        //切换
                        if (errCount >= errTotal && canSwitch == false)
                        {
                            foreach (var item in cdnList)
                            {
                                //排除本身
                                if (item.code.Equals(curCDN.content)) continue;
                                await _nightscoutServices.ChangeCDN(item.code);

                                try
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
                                    //异常推送
                                    if (pushUsers.Length > 0)
                                    {
                                        foreach (var userid in pushUsers)
                                        {
                                            var pushData = new WeChatCardMsgDataDto();
                                            pushData.cardMsg = new WeChatCardMsgDetailDto();
                                            pushData.cardMsg.keyword1 = $"NS网络异常,自动切换到:{item.name}";
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
                                //切换
                                canSwitch = true;
                                break;
                            }
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
