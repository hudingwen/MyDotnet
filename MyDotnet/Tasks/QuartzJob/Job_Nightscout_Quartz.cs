
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
using System.Linq;

namespace MyDotnet.Tasks.QuartzJob
{
    /// <summary>
    /// Nightscout到期提醒
    /// </summary>
    [JobDescriptionAttribute("Nightscout到期提醒", "会提前多少天提醒")]
    public class Job_Nightscout_Quartz : JobBase, IJob
    {
        public Job_Nightscout_Quartz(BaseServices<TasksQz> tasksQzServices
            , BaseServices<NightscoutServer> nightscoutServerServices
            , BaseServices<TasksLog> tasksLogServices
            , BaseServices<NightscoutLog> nightscoutLogServices
            , NightscoutServices nightscoutServices
            , WeChatConfigServices weChatConfigServices
            , DicService dicService
            ) : base(tasksQzServices, tasksLogServices)
        {
            _nightscoutServerServices = nightscoutServerServices;
            _nightscoutLogServices = nightscoutLogServices;
            _nightscoutServices = nightscoutServices;
            _weChatConfigServices = weChatConfigServices;
            _dicService = dicService;
        }

        public BaseServices<NightscoutServer> _nightscoutServerServices { get; set; }
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
                
                

                List<string> daoqi = new List<string>();
                int daoqiCount = 0;
                List<string> tixing = new List<string>();
                int tixingCount = 0;
                foreach (var nightscout in nights)
                {
                    try
                    {
                        
                        WeChatCardMsgDataDto pushData = null;
                        
                        if (DateTime.Now.Date >= nightscout.endTime)
                        {
                            pushData = new WeChatCardMsgDataDto();
                            pushData.cardMsg = new WeChatCardMsgDetailDto();
                            pushData.cardMsg.first = $"{nightscout.name},你的ns服务已到期(点我续费)";
                            daoqiCount += 1;
                        }
                        else if (DateTime.Now.Date.AddDays(preDay) >= nightscout.endTime)
                        {
                            pushData = new WeChatCardMsgDataDto();
                            pushData.cardMsg = new WeChatCardMsgDetailDto();
                            pushData.cardMsg.first = $"{nightscout.name},你的ns服务即将到期(点我续费)";
                            tixingCount += 1;
                        }

                        if (pushData != null)
                        {
                            var lessDays = Math.Ceiling((nightscout.endTime - DateTime.Now.Date).TotalDays);
                            if (lessDays <= 0)
                            {
                               

                                if (!nightscout.isStop)
                                {
                                    var nsserver = await _nightscoutServerServices.Dal.QueryById(nightscout.serverId);
                                    await _nightscoutServices.StopDocker(nightscout, nsserver);
                                    await _nightscoutServices.Dal.Db.Updateable<Nightscout>().SetColumns(t => t.isStop, true).Where(t => t.Id == nightscout.Id).ExecuteCommandAsync();
                                }
                                daoqi.Insert(0, nightscout.name);
                               
                                if (lessDays > -2 && lessDays < 0)
                                {
                                    pushData.cardMsg.keyword1 = $"NS已经到期,实例已停止服务(点我续费)";
                                }
                                else
                                {
                                    //到期后就不提醒了
                                    continue;
                                }


                                //var afterDayConfig = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.afterDays);
                                //var afterDay = afterDayConfig.content.ObjToInt() + lessDays;
                                //if (afterDay < 0)
                                //{
                                //    //删除
                                //    pushData.cardMsg.keyword1 = $"NS已经到期,服务已删除,请联系重新购买部署";
                                //    await _nightscoutServices.StopDocker(nightscout, nsserver);
                                //    await _nightscoutServices.DeleteData(nightscout, nsserver);
                                //    await _nightscoutServices.Dal.DeleteById(nightscout.Id);
                                //}
                                //else
                                //{
                                //    //停止
                                //    pushData.cardMsg.keyword1 = $"NS已经到期,请及时续费({afterDay}天后删除服务)";
                                //    if (!nightscout.isStop)
                                //    {
                                //        await _nightscoutServices.StopDocker(nightscout, nsserver);
                                //        await _nightscoutServices.Dal.Db.Updateable<Nightscout>().SetColumns(t => t.isStop, true).Where(t => t.Id == nightscout.Id).ExecuteCommandAsync();
                                //    }
                                //}

                            }
                            else
                            {
                                tixing.Insert(0, nightscout.name);
                                pushData.cardMsg.keyword1 = $"NS即将到期,{lessDays}天后停止服务(点我续费)";
                            }
                            
                            pushData.cardMsg.keyword2 = $"{nightscout.endTime.ToString("yyyy-MM-dd")}";
                            pushData.cardMsg.remark = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            pushData.cardMsg.url = $"{frontPage}?host={nightscout.url}";
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
                if (daoqi.Count>0 || tixing.Count>0)
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
                                pushData.cardMsg.keyword1 = $"NS用户：到期{daoqiCount}个，提醒{tixingCount}个";
                                daoqi.AddRange(tixing);
                                pushData.cardMsg.keyword2 = string.Join(",", daoqi.Take(10));
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
