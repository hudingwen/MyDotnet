
using MyDotnet.Domain.Dto.Other;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using Quartz;
using System.Net.Http.Headers;

namespace MyDotnet.Tasks.QuartzJob
{
    public class Job_QQGroup_Quartz : JobBase, IJob
    {
        public Job_QQGroup_Quartz(BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices
            ) : base(tasksQzServices, tasksLogServices)
        {
        }

        public async Task Execute(IJobExecutionContext context)
        {
            //var param = context.MergedJobDataMap;
            // 可以直接获取 JobDetail 的值
            var jobKey = context.JobDetail.Key;
            var jobId = jobKey.Name;
            var executeLog = await ExecuteJob(context, async () => await Run(context, jobId.ObjToLong()));

        }
        public async Task Run(IJobExecutionContext context, long jobid)
        {
            if (jobid > 0)
            {
                try
                {
                    var sendMsg = $"[CQ:at,qq=all] \n 28分小游戏,开搞啦开搞啦.北京时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
                    var groupid = 894466423;
                    var token = "";
                    var sendUrl = "";


                    string requestJson;
                    GrouInfo sendObj = new GrouInfo();
                    sendObj.auto_escape = false;
                    var tempMsg = sendMsg;
                    sendObj.message = tempMsg;
                    sendObj.group_id = groupid;
                    requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(sendObj);

                    string result = string.Empty;
                    using (HttpContent httpContent = new StringContent(requestJson))
                    {
                        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        using var httpClient = new HttpClient();
                        httpClient.DefaultRequestHeaders.Add("Authorization", token);
                        //httpClient.Timeout = TimeSpan.FromSeconds(60);
                        result = await httpClient.PostAsync(sendUrl + "/send_group_msg", httpContent).Result.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }

  

}
