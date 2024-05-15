
using MyDotnet.Domain.Dto.Trojan;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Domain.Entity.Trojan;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using MyDotnet.Services.System;
using Quartz;
using SixLabors.ImageSharp.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace MyDotnet.Tasks.QuartzJob
{
    public class Job_Trojan_Select_Quartz : JobBase, IJob
    {
        public Job_Trojan_Select_Quartz(BaseServices<TasksQz> tasksQzServices
            , BaseServices<TasksLog> tasksLogServices
            , UnitOfWorkManage unitOfWorkManage
            , BaseServices<TrojanServers> trojanServer
            , DicService dicService
            ) : base(tasksQzServices, tasksLogServices)
        {
            _unitOfWorkManage = unitOfWorkManage;
            _dicService = dicService;
            _trojanServer = trojanServer;

        }
        public DicService _dicService;
        public UnitOfWorkManage _unitOfWorkManage { get; set; } 
        public BaseServices<TrojanServers> _trojanServer { get; set; }

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
                var trojanAwsFastCDN = await _dicService.GetDicData(TrojanInfo.TrojanAwsFastCDN);
                var trojanAwsIP = await _dicService.GetDicData(TrojanInfo.TrojanAwsIP);
                var codes =  trojanAwsFastCDN.Select(t=>t.code).ToList();
                var servers = await _trojanServer.Dal.Query(t => codes.Contains(t.servercode));

                var ping = new Ping();
                PingOptions options = new PingOptions
                {
                    // 不允许分片
                    DontFragment = true
                };
                int timeout = 500; // 设置超时时间，单位为毫秒

                List<TrojanServers> updateServers = new List<TrojanServers>();
                foreach (var aws in trojanAwsFastCDN)
                {
                    string code = aws.code;
                    string host = aws.content;
                    var codeServers = servers.FindAll(t => code.Equals(t.servercode));

                    List<DelayCheckDto> delayCheckDtos = new List<DelayCheckDto>();
                    var ips = trojanAwsIP.FindAll(t => t.code.Equals(code));
                    foreach (var ipDic in ips)
                    {
                        var ipRange = ipDic.content;
                        var ipList = GetIpRange(ipRange);

                        foreach (var ipAddress in ipList)
                        {
                            var ip = ipAddress.ToString();
                            var reply = await ping.SendPingAsync(ip, timeout, new byte[32], options);

                            if (reply.Status == IPStatus.Success)
                            {
                                if (reply.RoundtripTime <= 100)
                                {
                                    delayCheckDtos.Add(new DelayCheckDto { ip = ip, delay = reply.RoundtripTime });
                                }
                            }
                        }
                    }
                    var fastList = delayCheckDtos.OrderBy(t => t.delay).Take(codeServers.Count).ToList();

                    for (int i = 0; i < fastList.Count; i++)
                    {

                        if (codeServers.Count - 1 >= i)
                        {
                            //范围内
                            var server = codeServers[i];
                            var fast = fastList[i];
                            server.serveraddress = fast.ip;
                            server.serverpeer = host;
                            updateServers.Add(server);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                await _trojanServer.Dal.Update(updateServers, t => new { t.serveraddress, t.serverpeer });
            }
        }

        List<IPAddress> GetIpRange(string subnet)
        {
            var parts = subnet.Split('/');
            var baseIp = IPAddress.Parse(parts[0]);
            var maskLength = int.Parse(parts[1]);

            var ipList = new List<IPAddress>();

            uint baseIpAsUint = IpToUint(baseIp);
            uint mask = 0xFFFFFFFF << (32 - maskLength);

            for (uint i = 1; i < (~mask); i++)
            {
                ipList.Add(UintToIp(baseIpAsUint + i));
            }

            return ipList;
        }

        uint IpToUint(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        IPAddress UintToIp(uint ipUint)
        {
            var bytes = BitConverter.GetBytes(ipUint);
            Array.Reverse(bytes);
            return new IPAddress(bytes);
        }
    }



}
