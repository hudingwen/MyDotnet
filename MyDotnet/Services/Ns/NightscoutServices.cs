using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using Renci.SshNet;
using System;
using System.Text;

namespace MyDotnet.Services.Ns
{
    /// <summary>
    /// NightscoutServices
    /// </summary>	
    public class NightscoutServices : BaseServices<Nightscout>
    {
        public NightscoutServices(BaseRepository<Nightscout> baseRepository
            , BaseServices<NightscoutLog> nightscoutLogServices
            , BaseServices<NightscoutServer> nightscoutServerServices
            ) : base(baseRepository)
        {
            _nightscoutLogServices = nightscoutLogServices;
            _nightscoutServerServices = nightscoutServerServices;
        }
        public BaseServices<NightscoutLog> _nightscoutLogServices { get; set; }
        public BaseServices<NightscoutServer> _nightscoutServerServices { get; set; }
        /// <summary>
        /// 添加解析
        /// </summary>
        /// <param name="nightscout"></param>
        /// <returns></returns>
        public async Task<bool> ResolveDomain(Nightscout nightscout)
        {
            NightscoutLog log = new NightscoutLog();
            log.success = true;

            await UnResolveDomain(nightscout);
            if (!NsInfo.defaultCDN.Equals(nightscout.cdn))
            {
                //非默认cdn添加解析 

                var selectCDN = NsInfo.CDNList.Find(t => t.key.Equals(nightscout.cdn));
                if(selectCDN != null)
                {
                    //添加
                    var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.cloudflare.com/client/v4/zones/{NsInfo.cfZoomID}/dns_records");
                    request.Headers.Add("Authorization", $"Bearer {NsInfo.cfKey}");
                    CFAddMessageInfo cfAdd = new CFAddMessageInfo();
                    cfAdd.content = selectCDN.value;
                    cfAdd.name = nightscout.url;
                    cfAdd.proxied = false;
                    cfAdd.type = selectCDN.type;
                    cfAdd.comment = "自动创建解析";
                    cfAdd.ttl = 60;
                    var content = new StringContent(JsonHelper.ObjToJson(cfAdd), null, "text/plain");
                    request.Content = content;
                    var txt = await HttpHelper.SendAsync(request);
                    var obj = JsonHelper.JsonToObj<CFMessageInfo>(txt);
                    if (obj.success)
                    {
                        log.content = "添加解析成功";
                    }
                    else
                    {
                        log.content = "添加解析失败";
                    }
                    log.pid = nightscout.Id;
                    log.success = obj.success;
                    return obj.success;
                }
                else
                {
                    log.content = "添加默认解析";
                }
            }
            else
            {
                log.content = "添加默认解析";
            }
            try
            {
                await _nightscoutLogServices.Dal.Add(log);
            }
            catch (Exception ex)
            {
                LogHelper.logSys.Error("ns日志记录失败", ex);
            }
            return true;
        }
        /// <summary>
        /// 删除解析
        /// </summary>
        /// <param name="nightscout"></param>
        /// <returns></returns>
        public async Task<bool> UnResolveDomain(Nightscout nightscout)
        {
            NightscoutLog log = new NightscoutLog();
            //type=CNAME&
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.cloudflare.com/client/v4/zones/{NsInfo.cfZoomID}/dns_records?name={nightscout.url}");
            request.Headers.Add("Authorization", $"Bearer {NsInfo.cfKey}");
            var txt = await HttpHelper.SendAsync(request);
            var obj = JsonHelper.JsonToObj<CFMessageListInfo>(txt);

            if (obj.success && obj.result != null && obj.result.Count == 1)
            {
                //删除
                request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.cloudflare.com/client/v4/zones/{NsInfo.cfZoomID}/dns_records/{obj.result[0].id}");
                request.Headers.Add("Authorization", $"Bearer {NsInfo.cfKey}");
                txt = await HttpHelper.SendAsync(request);
                var obj2 = JsonHelper.JsonToObj<CFMessageInfo>(txt);
                if (obj2.success)
                {
                    log.content = "删除解析成功";
                }
                else
                {
                    log.content = "删除解析失败";
                }
            }
            else
            {
                log.content = "没有要删除的解析";
            }
            log.pid = nightscout.Id;
            log.success = obj.success;
            try
            {
                await _nightscoutLogServices.Dal.Add(log);
            }
            catch (Exception ex)
            {
                LogHelper.logSys.Error("ns日志记录失败", ex);
            }
            return obj.success;
        }

        public async Task InitData(Nightscout nightscout, NightscoutServer nsserver)
        {
            if (string.IsNullOrEmpty(nightscout.serviceName) || string.IsNullOrEmpty(nightscout.url)) return;
            //获取mongo数据库
            var master = (await _nightscoutServerServices.Dal.Query(t => t.isMongo == true)).FirstOrDefault();
            NightscoutLog log = new NightscoutLog();
            StringBuilder sb = new StringBuilder();
            try
            {
                using (var sshClient = new SshClient(master.serverIp, master.serverPort, master.serverLoginName, master.serverLoginPassword))
                {
                    //创建SSH
                    sshClient.Connect();

                    using (var cmd = sshClient.CreateCommand(""))
                    {
                        //创建用户
                        var grantConnectionMongoString = $"mongodb://{nsserver.mongoLoginName}:{nsserver.mongoLoginPassword}@{nsserver.mongoIp}:{nsserver.mongoPort}";
                        var client = new MongoClient(grantConnectionMongoString);

                        //try
                        //{
                        //    client.DropDatabase(nightscout.serviceName);
                        //}
                        //catch (Exception ex)
                        //{
                        //    sb.Append($"删除数据库失败:{ex.Message}");
                        //}
                        var database = client.GetDatabase(nightscout.serviceName);

                        //try
                        //{
                        //    var deleteUserCommand = new BsonDocument
                        //    {
                        //        { "dropUser", nsserver.mongoLoginName },
                        //        { "writeConcern", new BsonDocument("w", 1) }
                        //    };
                        //    // 执行删除用户的命令
                        //    var result = database.RunCommand<BsonDocument>(deleteUserCommand);

                        //}
                        //catch (Exception ex)
                        //{
                        //    sb.Append($"删除用户失败:{ex.Message}");
                        //}
                        try
                        {
                            //创建用户
                            var command = new BsonDocument
                                    {
                                        { "createUser", nsserver.mongoLoginName },
                                        { "pwd" ,nsserver.mongoLoginPassword },
                                        { "roles", new BsonArray
                                            {
                                                {"readWrite"}
                                            }
                                        }
                                    };
                            var result = database.RunCommand<BsonDocument>(command);
                        }
                        catch (Exception ex)
                        {
                            sb.Append($"创建用户失败:{ex.Message}");
                        }

                        //初始化数据库
                        var res = cmd.Execute($"docker exec -t mongoserver mongorestore -u={nsserver.mongoLoginName} -p={nsserver.mongoLoginPassword} -d {nightscout.serviceName} /data/backup/template");

                        try
                        {
                            //修改参数
                            var collection = database.GetCollection<MongoDB.Bson.BsonDocument>("profile"); // 集合
                            var filter = Builders<MongoDB.Bson.BsonDocument>.Filter.Empty; // 条件
                            DateTime date = DateTime.Now.ToUniversalTime(); // 获取当前日期和时间的DateTime对象
                            long timestamp = (long)(date - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                            var update = Builders<BsonDocument>.Update
                                .Set("created_at", date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                                .Set("mills", timestamp)
                                .Set("startDate", date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                            var updateRes = collection.UpdateOne(filter, update);
                        }
                        catch (Exception ex)
                        {
                            sb.Append($"创建初始化数据失败:{ex.Message}");
                        }
                    }

                    sshClient.Disconnect();
                }
                log.success = true;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"初始化失败:{ex.Message}");
                LogHelper.logApp.Error("初始化失败",ex);
                log.success = false;
                throw;
            }
            finally
            {
                log.content = sb.ToString();
                log.pid = nightscout.Id;
                try
                {
                    await _nightscoutLogServices.Dal.Add(log);
                }
                catch (Exception ex)
                {
                    LogHelper.logSys.Error("ns日志记录失败", ex);
                }
            }
        }

        public async Task StopDocker(Nightscout nightscout, NightscoutServer nsserver)
        {
            if (string.IsNullOrEmpty(nightscout.serviceName) || string.IsNullOrEmpty(nightscout.url)) return;
            NightscoutLog log = new NightscoutLog();
            StringBuilder sb = new StringBuilder();


            try
            {
                FileHelper.FileDel($"/etc/nginx/conf.d/nightscout/{nightscout.Id}.conf");
                using (var sshClient = new SshClient(nsserver.serverIp, nsserver.serverPort, nsserver.serverLoginName, nsserver.serverLoginPassword))
                {
                    //创建SSH
                    sshClient.Connect();
                    using (var cmd = sshClient.CreateCommand(""))
                    {
                        //刷新nginx
                        var master = (await _nightscoutServerServices.Dal.Query(t => t.isNginx == true)).FirstOrDefault();
                        if (master != null)
                        {
                            using (var sshMasterClient = new SshClient(master.serverIp, master.serverPort, master.serverLoginName, master.serverLoginPassword))
                            {
                                sshMasterClient.Connect();
                                using (var cmdMaster = sshMasterClient.CreateCommand(""))
                                {
                                    Thread.Sleep(1000);
                                    var resMaster = cmdMaster.Execute("docker exec -t nginxserver nginx -s stop");
                                    Thread.Sleep(3000);
                                    resMaster += cmdMaster.Execute("docker exec -t nginxserver nginx -s reload");
                                    Thread.Sleep(1000);

                                    sb.AppendLine($"刷新域名:{resMaster}");
                                }
                                sshMasterClient.Disconnect();
                            }
                        }
                        else
                        {
                            sb.AppendLine("没有找到nginx服务器");
                        }
                        //停止实例
                        var res = cmd.Execute($"docker stop {nightscout.serviceName}");
                        sb.AppendLine($"停止实例:{res}");

                        //删除实例
                        res = cmd.Execute($"docker rm {nightscout.serviceName}");
                        sb.AppendLine($"删除实例:{res}");
                    }
                    sshClient.Disconnect();
                }
                log.success = true;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"停止实例错误:{ex.Message}");
                log.success = false;
                LogHelper.logApp.Error(ex.Message);
                LogHelper.logApp.Error(ex.StackTrace);
                throw;
            }
            finally
            {
                log.content = sb.ToString();
                log.pid = nightscout.Id;
                try
                {
                    await _nightscoutLogServices.Dal.Add(log);
                }
                catch (Exception ex)
                {
                    LogHelper.logSys.Error("ns日志记录失败", ex);
                }
            }


        }

        public async Task DeleteData(Nightscout nightscout, NightscoutServer nsserver)
        {
            if (string.IsNullOrEmpty(nightscout.serviceName) || string.IsNullOrEmpty(nightscout.url)) return;

            NightscoutLog log = new NightscoutLog();
            StringBuilder sb = new StringBuilder();


            try
            {
                FileHelper.FileDel($"/etc/nginx/conf.d/nightscout/{nightscout.Id}.conf");

                var connectionMongoString = $"mongodb://{nsserver.mongoLoginName}:{nsserver.mongoLoginPassword}@{nsserver.mongoIp}:{nsserver.mongoPort}";
                var client = new MongoClient(connectionMongoString);

                var database = client.GetDatabase(nightscout.serviceName);
                var deleteUserCommand = new BsonDocument
                    {
                        { "dropUser", nsserver.mongoLoginName },
                        { "writeConcern", new BsonDocument("w", 1) }
                    };
                // 执行删除用户的命令
                try
                {
                    var delUserInfo = database.RunCommand<BsonDocument>(deleteUserCommand);
                    sb.AppendLine(delUserInfo == null ? "" : delUserInfo.ToJson());
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"删除mongo用户失败:{ex.Message}");
                }
                //删除mongo
                try
                {
                    client.DropDatabase(nightscout.serviceName);
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"删除mongo数据库失败:{ex.Message}");
                }
                log.success = true;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"删除数据错误:{ex.Message}");
                log.success = false;
                LogHelper.logApp.Error("删除实例错误", ex);
                throw;
            }
            finally
            {
                log.content = sb.ToString();
                log.pid = nightscout.Id;
                await _nightscoutLogServices.Dal.Add(log);
            }
        }

        public async Task RunDocker(Nightscout nightscout, NightscoutServer nsserver)
        {
            try
            {
                if (string.IsNullOrEmpty(nightscout.serviceName) || string.IsNullOrEmpty(nightscout.url)) return;

                NightscoutLog log = new NightscoutLog();
                StringBuilder sb = new StringBuilder();
                try
                {
                    string webConfig = GetNsWebConfig(nightscout, nsserver);

                    FileHelper.WriteFile($"/etc/nginx/conf.d/nightscout/{nightscout.Id}.conf", webConfig);



                    //这儿会有两种安装方式
                    //一是按服务器的IP+端口部署(ns和nginx在同一服务器)
                    //二是按本机实例IP+1337端口部署



                    using (var sshClient = new SshClient(nsserver.serverIp, nsserver.serverPort, nsserver.serverLoginName, nsserver.serverLoginPassword))
                    {
                        //创建SSH
                        sshClient.Connect();

                        using (var cmd = sshClient.CreateCommand(""))
                        {


                            //停止实例
                            var res = cmd.Execute($"docker stop {nightscout.serviceName}");
                            sb.AppendLine($"停止实例:{res}");

                            //删除实例
                            res = cmd.Execute($"docker rm {nightscout.serviceName}");
                            sb.AppendLine($"删除实例:{res}");

                            //启动实例
                            string cmdStr = GetNsDockerConfig(nightscout, nsserver);
                            res = cmd.Execute(cmdStr);
                            sb.AppendLine($"启动实例:{res}");

                            //刷新nginx
                            var master = (await _nightscoutServerServices.Dal.Query(t => t.isNginx == true)).FirstOrDefault();
                            if (master != null)
                            {
                                using (var sshMasterClient = new SshClient(master.serverIp, master.serverPort, master.serverLoginName, master.serverLoginPassword))
                                {
                                    sshMasterClient.Connect();
                                    using (var cmdMaster = sshMasterClient.CreateCommand(""))
                                    {
                                        Thread.Sleep(1000);
                                        var resMaster = cmdMaster.Execute("docker exec -t nginxserver nginx -s stop");
                                        Thread.Sleep(3000);
                                        resMaster += cmdMaster.Execute("docker exec -t nginxserver nginx -s reload");
                                        Thread.Sleep(1000);

                                        sb.AppendLine($"刷新域名:{resMaster}");
                                    }
                                    sshMasterClient.Disconnect();
                                }
                            }
                            else
                            {
                                sb.AppendLine("没有找到nginx服务器");
                            }
                        }

                        sshClient.Disconnect();
                    }
                    log.success = true;
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"启动实例错误:{ex.Message}");
                    log.success = false;
                    throw;
                }
                finally
                {
                    log.content = sb.ToString();
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
            catch (Exception ex)
            {
                LogHelper.logApp.Error("启动实例错误", ex);
                throw;
            }
        }
        /// <summary>
        /// 获取ns容器配置
        /// </summary>
        /// <param name="nightscout"></param>
        /// <param name="nsserver"></param>
        /// <returns></returns>
        public string GetNsDockerConfig(Nightscout nightscout, NightscoutServer nsserver)
        {
            List<string> args = new List<string>();
            if (nightscout.exposedPort > 0)
            {
                //外网
                args.Add($"docker run -m 100m --cpus=1 --restart=always -p {nightscout.exposedPort}:1337 --name {nightscout.serviceName}");
            }
            else
            {
                //内网
                args.Add($"docker run -m 100m --cpus=1 --restart=always --net mynet --ip {nightscout.instanceIP} --name {nightscout.serviceName}");
            }
            args.Add($"-e TZ=Asia/Shanghai");
            args.Add($"-e NODE_ENV=production");
            args.Add($"-e INSECURE_USE_HTTP='true'");

            //数据库链接
            var connectionMongoString = $"mongodb://{nsserver.mongoLoginName}:{nsserver.mongoLoginPassword}@{nsserver.mongoIp}:{nsserver.mongoPort}/{nightscout.serviceName}";

            args.Add($"-e MONGO_CONNECTION={connectionMongoString}");
            args.Add($"-e API_SECRET={nightscout.passwd}");
            //args.Add($"-v {path}/logo2.png:/opt/app/static/images/logo2.png");
            //args.Add($"-v {path}/boluswizardpreview.js:/opt/app/lib/plugins/boluswizardpreview.js");
            //args.Add($"-v {path}/sandbox.js:/opt/app/lib/sandbox.js");
            //args.Add($"-v {path}/constants.json:/opt/app/lib/constants.json");
            //args.Add($"-v {path}/zh_CN.json:/opt/app/translations/zh_CN.json");
            //args.Add($"-v {path}/maker.js:/opt/app/lib/plugins/maker.js");
            //args.Add($"-v {path}/hashauth.js:/opt/app/lib/client/hashauth.js");
            //args.Add($"-v {path}/enclave.js:/opt/app/lib/server/enclave.js");
            if (nightscout.isConnection)
            {
                args.Add($"-e MAKER_KEY={NsInfo.MAKER_KEY}");
                if (nightscout.isKeepPush)
                {
                    args.Add($"-e KEEP_PUSH='true'");
                }
                args.Add($"-e PUSH_URL='{NsInfo.pushUrl}'");
            }
            args.Add($"-e LANGUAGE=zh_cn");
            args.Add($"-e DISPLAY_UNITS='mmol/L'");
            args.Add($"-e TIME_FORMAT=24");
            args.Add($"-e CUSTOM_TITLE='{NsInfo.CUSTOM_TITLE}'");
            args.Add($"-e THEME=colors");


            List<string> pluginsArr;
            try
            {
                var pluginsNights = JsonHelper.JsonToObj<List<string>>(nightscout.plugins.ObjToString());
                if (pluginsNights != null && pluginsNights.Count > 0)
                {
                    pluginsArr = pluginsNights;
                }
                else
                {
                    pluginsArr = NsInfo.plugins.Select(t => t.key).ToList();
                }
            }
            catch (Exception)
            {
                pluginsArr = NsInfo.plugins.Select(t => t.key).ToList();
            }
            args.Add($"-e SHOW_PLUGINS='{string.Join(" ", pluginsArr)}'");
            args.Add($"-e ENABLE='{string.Join(" ", pluginsArr)}'");

            //args.Add($"-e SHOW_PLUGINS='careportal basal dbsize rawbg iob maker cob bridge bwp cage iage sage boluscalc pushover treatmentnotify mmconnect loop pump profile food openaps bage alexa override cors'");
            //args.Add($"-e ENABLE='careportal basal dbsize rawbg iob maker cob bridge bwp cage iage sage boluscalc pushover treatmentnotify mmconnect loop pump profile food openaps bage alexa override cors'");

            args.Add($"-e AUTH_DEFAULT_ROLES=readable");
            args.Add($"-e uid={nightscout.Id}");

            //苹果远程指令

            args.Add($"-e LOOP_APNS_KEY_ID='{NsInfo.apKeyID}'");
            args.Add($"-e LOOP_APNS_KEY='{NsInfo.apKey}'");
            args.Add($"-e LOOP_DEVELOPER_TEAM_ID='{NsInfo.apTeamID}'");
            args.Add($"-e LOOP_PUSH_SERVER_ENVIRONMENT='{NsInfo.apEnv}'");

            //args.Add($"-d nightscout/cgm-remote-monitor:latest");
            args.Add($"-d {NsInfo.image}");

            var cmdStr = string.Join(" ", args);
            return cmdStr;
        }

        /// <summary>
        /// 获取ns网站代理配置
        /// </summary>
        /// <param name="nightscout"></param>
        /// <param name="nsserver"></param>
        /// <returns></returns>
        public string GetNsWebConfig(Nightscout nightscout, NightscoutServer nsserver)
        {
            return @$"
server {{
    listen 443 ssl http2;
    server_name {nightscout.url} {nightscout.backupurl};

    ssl_certificate ""/etc/nginx/conf.d/{NsInfo.cer}"";
    ssl_certificate_key ""/etc/nginx/conf.d/{NsInfo.key}"";

    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ALL:!DH:!EXPORT:!RC4:+HIGH:+MEDIUM:!eNULL;
    ssl_prefer_server_ciphers on;

    location / {{
        proxy_set_header   Host             $host;
        proxy_set_header   X-Real-IP        $remote_addr;
        proxy_set_header   X-Forwarded-For  $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
        proxy_redirect off;
        proxy_http_version 1.1;
        proxy_pass http://{(nightscout.exposedPort > 0 ? nsserver.serverIp : nightscout.instanceIP)}:{(nightscout.exposedPort > 0 ? nightscout.exposedPort : 1337)};
    }}
    
}}
";
        }

        /// <summary>
        /// 删除上个月及以前的数据
        /// </summary>
        /// <returns></returns>
        public async Task ClearMongoData(Nightscout nightscout)
        {

            try
            {
                //Builders<BsonDocument>.Filter.Gt("date", 0) &
                var flagTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                flagTime = flagTime.AddMilliseconds(-1).ToUniversalTime();


                var client = new MongoClient($"mongodb://{NsInfo.miniLoginName}:{NsInfo.miniLoginPasswd}@{NsInfo.miniHost}:{NsInfo.miniPort}"); // 连接到MongoDB
                var database = client.GetDatabase(nightscout.serviceName); // 获取数据库对象



                var collectionEntries = database.GetCollection<BsonDocument>("entries"); // 替换为你的集合名称
                var filterEntries = Builders<BsonDocument>.Filter.Lte("date", (flagTime - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);
                //var ls = await collectionEntries.Find(filterEntries).ToListAsync();
                collectionEntries.DeleteMany(filterEntries); // 删除匹配的数据
                                                             

                var collectionDevicestatus = database.GetCollection<BsonDocument>("devicestatus"); // 替换为你的集合名称
                var filterDevicestatus = Builders<BsonDocument>.Filter.Lte("created_at", flagTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                //var ls = await collectionDevicestatus.Find(filterDevicestatus).ToListAsync();
                collectionDevicestatus.DeleteMany(filterDevicestatus); // 删除匹配的数据

                var collectionTreatments = database.GetCollection<BsonDocument>("treatments"); // 替换为你的集合名称
                var filterTreatments = Builders<BsonDocument>.Filter.Lte("created_at", flagTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                //var ls = await collectionTreatments.Find(filterTreatments).ToListAsync();
                collectionTreatments.DeleteMany(filterTreatments); // 删除匹配的数据


                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogHelper.logApp.Error($"ns用户({nightscout.Id})数据删除失败:{ex.Message}", ex);
            }


        }
    }

    
}