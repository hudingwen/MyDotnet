﻿using MyDotnet.Domain.Attr;
using MyDotnet.Domain.Dto.BTC;
using MyDotnet.Domain.Dto.WeChat;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.System;
using MyDotnet.Services.WeChat;
using Quartz;

namespace MyDotnet.Tasks.QuartzJob
{
    /// <summary>
    /// Nightscout定时切换CDN
    /// </summary>
    [JobDescriptionAttribute("比特币钱余额监控", "有余额变化发出通知")]
    public class Job_My_BTC_Quartz : JobBase, IJob
    {
        public Job_My_BTC_Quartz(BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices
            , DicService dicService
            , WeChatConfigServices weChatConfigServices
            ) : base(tasksQzServices, tasksLogServices)
        {
            _dicService = dicService;
            _weChatConfigServices = weChatConfigServices;
        }
        public DicService _dicService;
        public WeChatConfigServices _weChatConfigServices { get; set; }


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


                //web列表 
                var publicAccount = await _dicService.GetDicData(MyBTC.KEY, MyBTC.publicAccount);
                var publicCompany = await _dicService.GetDicData(MyBTC.KEY, MyBTC.publicCompany);
                var publicUser = await _dicService.GetDicData(MyBTC.KEY, MyBTC.publicUser);
                var publicTemplate = await _dicService.GetDicData(MyBTC.KEY, MyBTC.publicTemplate); 

                var model = await _tasksQzServices.Dal.QueryById(jobid);

                var wallet =  await HttpHelper.GetAsync($"https://api.blockcypher.com/v1/btc/main/addrs/{model.JobParams.ObjToString().Trim()}/balance");

                

                var walletInfo =  JsonHelper.JsonToObj<BtcWalletInfo>(wallet);
                if (string.IsNullOrEmpty(walletInfo.address)) throw new Exception($"请求失败:{wallet}");
                var myMoney = walletInfo.final_balance.ObjToLong() / 100_000_000m;
                var oldMoney = model.StoreData.ObjToDecimal();
                if (!oldMoney.Equals(myMoney))
                {
                    model.StoreData = myMoney.ToString();
                    await _tasksQzServices.Dal.Update(model);
                    try
                    {
                        var pushData = new WeChatCardMsgDataDto();
                        pushData.cardMsg = new WeChatCardMsgDetailDto();
                        pushData.cardMsg.first = $"比特币余额变动通知:{oldMoney}=>{myMoney}"; 
                        pushData.cardMsg.url = "";
                        pushData.cardMsg.template_id = publicTemplate.content;
                        pushData.info = new WeChatUserInfo();
                        pushData.info.id = publicAccount.content;
                        pushData.info.companyCode = publicCompany.content;
                        pushData.info.userID = publicUser.content;
                        await _weChatConfigServices.PushCardMsg(pushData);

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
