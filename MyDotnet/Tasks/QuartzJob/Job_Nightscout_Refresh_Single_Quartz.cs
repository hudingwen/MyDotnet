
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.WeChat;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.Ns;
using MyDotnet.Services.WeChat;
using Quartz;
using Renci.SshNet;
using System.Text;

namespace MyDotnet.Tasks.QuartzJob
{
    /// <summary>
    /// Nightscout定时刷新-单个服务器(需参数)
    /// </summary>
    public class Job_Nightscout_Refresh_Single_Quartz : JobBase, IJob
    {
        public Job_Nightscout_Refresh_Single_Quartz(BaseServices<TasksQz> tasksQzServices
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
            if (jobid <= 0)
                return;
            JobDataMap data = context.JobDetail.JobDataMap;

            string pars = data.GetString("JobParam");
            var nsConfig = JsonHelper.JsonToObj<NightscoutRemindConfig>(pars);

            long serverId = nsConfig.serverId;
            var nsserver = await _nightscoutServerServices.Dal.QueryById(serverId);
            if (nsserver == null)
                throw new Exception($"服务器:{serverId}未找到");
            var nights = await _nightscoutServices.Dal.Query(t => t.serverId == nsserver.Id);


            //刷新nginx
            var master = (await _nightscoutServerServices.Dal.Query(t => t.isNginx == true)).FirstOrDefault();
            if (master == null)
            {
                LogHelper.logApp.InfoFormat($"每周ns重启任务出现失败:没有找到nginx服务器");
                return;
            }
            using (var sshMasterClient = new SshClient(master.serverIp, master.serverPort, master.serverLoginName, master.serverLoginPassword))
            {
                //创建SSH
                sshMasterClient.Connect();
                using (var cmdMaster = sshMasterClient.CreateCommand(""))
                {

                    List<string> errCount = new List<string>();
                    //单个服务器重启任务
                    try
                    {
                        using (var sshClient = new SshClient(nsserver.serverIp, nsserver.serverPort, nsserver.serverLoginName, nsserver.serverLoginPassword))
                        {
                            //创建SSH
                            sshClient.Connect();
                            using (var cmd = sshClient.CreateCommand(""))
                            {
                                StringBuilder sb = new StringBuilder();
                                foreach (var nightscout in nights)
                                {
                                    NightscoutLog log = new NightscoutLog();
                                    try
                                    {
                                        if (nightscout.isStop) continue;
                                        //删除域名
                                        FileHelper.FileDel($"/etc/nginx/conf.d/nightscout/{nightscout.Id}.conf");
                                        //停止实例
                                        var res = cmd.Execute($"docker stop {nightscout.serviceName}");
                                        sb.AppendLine($"停止实例:{res}");
                                        //删除实例
                                        res = cmd.Execute($"docker rm {nightscout.serviceName}");
                                        sb.AppendLine($"删除实例:{res}");
                                        //启动实例
                                        string cmdStr = _nightscoutServices.GetNsDockerConfig(nightscout, nsserver);
                                        res = cmd.Execute(cmdStr);
                                        sb.AppendLine($"启动实例:{res}");
                                        //添加域名
                                        string webConfig = _nightscoutServices.GetNsWebConfig(nightscout, nsserver);
                                        FileHelper.WriteFile($"/etc/nginx/conf.d/nightscout/{nightscout.Id}.conf", webConfig);
                                        //刷新域名
                                        Thread.Sleep(2000);
                                        var resMaster = cmdMaster.Execute("docker exec -t nginxserver nginx -s stop");
                                        Thread.Sleep(2000);
                                        resMaster += cmdMaster.Execute("docker exec -t nginxserver nginx -s reload");
                                        Thread.Sleep(2000);

                                        sb.AppendLine($"刷新域名:{resMaster}");
                                        log.success = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        log.success = false;
                                        errCount.Add(nightscout.name);
                                        LogHelper.logSys.Error($"{nightscout.name}-重启实例失败:{ex.Message}", ex);
                                        sb.AppendLine($"实例异常:{ex.Message}");
                                    }
                                    finally
                                    {
                                        log.content = sb.ToString();
                                        sb.Clear();
                                        log.pid = nightscout.Id;
                                        try
                                        {
                                            await _nightscoutLogServices.Dal.Add(log);
                                        }
                                        catch (Exception ex)
                                        {
                                            LogHelper.logSys.Error("ns日志记录失败", ex);
                                            LogHelper.logSys.Error($"ns日志记录失败:{log.content}-{log.pid}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.logSys.Error($"ns服务器:{nsserver.serverName}重启出现异常", ex);
                        errCount.Add($"{nsserver.serverName}");
                    }

                    try
                    {
                        var pushUsers = nsConfig.pushUserIDs.Split(",", StringSplitOptions.RemoveEmptyEntries);
                        if (pushUsers.Length > 0)
                        {
                            foreach (var userid in pushUsers)
                            {
                                var pushData = new WeChatCardMsgDataDto();
                                pushData.cardMsg = new WeChatCardMsgDetailDto();
                                if (errCount.Count > 0)
                                {
                                    pushData.cardMsg.keyword1 = $"单个ns重启任务出现失败:{errCount.Count}个";

                                    LogHelper.logApp.InfoFormat($"单个ns重启任务出现失败:{errCount.Count}个");
                                    LogHelper.logApp.InfoFormat($"失败名单:{string.Join(",", errCount)}");
                                }
                                else
                                {
                                    pushData.cardMsg.keyword1 = $"单个ns重启任务完成:{nsserver.serverName}";
                                }
                                pushData.cardMsg.keyword2 = string.Join(",", errCount);
                                pushData.cardMsg.remark = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                pushData.cardMsg.url = NsInfo.frontPage;
                                pushData.cardMsg.template_id = NsInfo.pushTemplateID_Alert;
                                pushData.info = new WeChatUserInfo();
                                pushData.info.id = NsInfo.pushWechatID;
                                pushData.info.companyCode = NsInfo.pushCompanyCode;
                                pushData.info.userID = userid;
                                await _weChatConfigServices.PushCardMsg(pushData);
                            }
                            LogHelper.logApp.InfoFormat($"每周ns重启任务出现失败:{errCount.Count}个");
                            LogHelper.logApp.InfoFormat($"失败名单:{string.Join(",", errCount)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.logSys.Error($"推送失败,{ex.Message}", ex);
                    }
                }
                sshMasterClient.Disconnect();
            }





        }
    }


}
