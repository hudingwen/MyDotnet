
using Amazon.Runtime.Internal.Util;
using MyDotnet.Domain.Attr;
using MyDotnet.Domain.Dto.Apple;
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
using System.Security.Cryptography;

namespace MyDotnet.Tasks.QuartzJob
{
    /// <summary>
    /// 苹果设备审中设备查询
    /// </summary>
    [JobDescriptionAttribute("苹果设备审中设备查询", "审核完成后通知")]
    public class Job_Nightscout_Apple_Quartz : JobBase, IJob
    {
        public Job_Nightscout_Apple_Quartz(BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices
            , WeChatConfigServices weChatConfigServices
            , DicService dicService
            ) : base(tasksQzServices, tasksLogServices)
        {
            _weChatConfigServices = weChatConfigServices;
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
        /// <summary>
        /// 审核中的设备
        /// </summary>
        public Dictionary<string,List<string>> processingDevices = new Dictionary<string, List<string>>();
        public async Task Run(IJobExecutionContext context, long jobid)
        {
            if (jobid > 0)
            {

                //是否结束任务 1-运行 2-退出
                string existStatus = "1";

                while (true && "1".Equals(existStatus))
                {


                    var nsInfo = await _dicService.GetDicData(NsInfo.KEY);
                    var frontPage = nsInfo.Find(t => t.code.Equals(NsInfo.frontPage)).content;
                    var pushTemplateID_Alert = nsInfo.Find(t => t.code.Equals(NsInfo.pushTemplateID_Alert)).content;
                    var pushWechatID = nsInfo.Find(t => t.code.Equals(NsInfo.pushWechatID)).content;
                    var pushCompanyCode = nsInfo.Find(t => t.code.Equals(NsInfo.pushCompanyCode)).content;

                    try
                    {
                        //查询苹果
                        var appleApiList = await _dicService.GetDicData(DicAppleInfo.AppleApiList);
                        if (appleApiList != null && appleApiList.Count > 0)
                        {
                            var preInnerUser = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.preInnerUser);
                            var pushUsers = preInnerUser.content.Split(",", StringSplitOptions.RemoveEmptyEntries);
                            foreach (var apple in appleApiList)
                            {
                                var token = AppleHelper.GetNewAppleToken(apple.code, apple.content, apple.content2);
                                var list = await AppleHelper.GetDevices(token, 1, 200, "PROCESSING");

                                //先查找谁审核通过了
                                List<string> hasDevices = null;
                                processingDevices.TryGetValue(apple.code, out hasDevices);
                                if (hasDevices == null)
                                {
                                    processingDevices.Add(apple.code, new List<string>());
                                    processingDevices.TryGetValue(apple.code, out hasDevices);
                                }
                                var hasSendDevices = new List<string>();
                                foreach (var udid in hasDevices)
                                {
                                    var findUdid = list.data.Find(t => t.attributes.udid.Equals(udid));
                                    if (findUdid == null)
                                    {
                                        //审核通知推送
                                        if (pushUsers.Length > 0)
                                        {
                                            foreach (var userid in pushUsers)
                                            {
                                                var pushData = new WeChatCardMsgDataDto();
                                                pushData.cardMsg = new WeChatCardMsgDetailDto();
                                                pushData.cardMsg.keyword1 = $"设备审核完成:{udid}";
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
                                        hasSendDevices.Add(udid);
                                    }
                                }
                                //去除审核完成的
                                foreach (var udid in hasSendDevices)
                                {
                                    hasDevices.Remove(udid);
                                }

                                //再加入审核中的设备
                                foreach (var device in list.data)
                                {
                                    var findAddDevice = hasDevices.Find(t => t.Equals(device.attributes.udid));
                                    if (findAddDevice == null)
                                    {
                                        hasDevices.Add(device.attributes.udid);
                                    }
                                }
                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.logApp.Error("苹果查询异常", ex);
                    }
                    finally
                    { 
                        //查询配置
                        var stepTimeDic = await _dicService.GetDicDataOne(DicAppleInfo.AppleApiConfig, DicAppleInfo.AppleApiConfig_stepTimes);
                        var checkFinish = await _dicService.GetDicDataOne(DicAppleInfo.AppleApiConfig, DicAppleInfo.AppleApiConfig_checkIsExis);
                        //间隔
                        Thread.Sleep(stepTimeDic.content.ObjToInt() * 1000 * 60);
                        //主动退出
                        existStatus = checkFinish.content;
                    }
                }
                
            }
        }
    }


}
