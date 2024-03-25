using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MyDotnet.Controllers.Ns;
using MyDotnet.Domain.Dto.ExceptionDomain;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using MyDotnet.Services.System;
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
         
        public BaseServices<NightscoutLog> _nightscoutLogServices { get; set; }
        public BaseServices<NightscoutServer> _nightscoutServerServices { get; set; }
        public BaseServices<NightscoutCustomer> _nightscoutCustomerServices { get; set; }
        public DicService _dicService {  get; set; }

        public NightscoutServices(BaseRepository<Nightscout> baseRepository
            , BaseServices<NightscoutLog> nightscoutLogServices
            , BaseServices<NightscoutServer> nightscoutServerServices
            , BaseServices<NightscoutCustomer> nightscoutCustomerServices
            , DicService dicService
            ) : base(baseRepository)
        {
            _nightscoutLogServices = nightscoutLogServices;
            _nightscoutServerServices = nightscoutServerServices;
            _nightscoutCustomerServices = nightscoutCustomerServices;
            _dicService = dicService;
        }

        /// <summary>
        /// 修改默认cdn
        /// </summary>
        /// <param name="cdnCode"></param>
        /// <returns></returns>
        public async Task ChangeCDN(string cdnCode)
        {

            DicType defaultCDN = await _dicService.GetDic(DicTypeList.defaultCDN);

            var nsInfo = await _dicService.GetDicData(NsInfo.KEY);
            var genericUrl = nsInfo.Find(t => NsInfo.genericUrl.Equals(t.code)).content;


            var cdnKey = await _dicService.GetDicData(CDNKey.KEY);
            var cfZoom = cdnKey.Find(t => CDNKey.zoomID.Equals(t.code)).content;
            var cfKey = cdnKey.Find(t => CDNKey.key.Equals(t.code)).content;


            var cdnList = await _dicService.GetDicData(CDNList.KEY);

            if (!defaultCDN.content.Equals(cdnCode))
            {
                //删除
                NightscoutLog log = new NightscoutLog();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.cloudflare.com/client/v4/zones/{cfZoom}/dns_records?name={genericUrl}");
                request.Headers.Add("Authorization", $"Bearer {cfKey}");
                var findtxt = await HttpHelper.SendAsync(request);
                var findInfo = JsonHelper.JsonToObj<CFMessageListInfo>(findtxt);

                bool isDelete = false;
                if (findInfo.success && findInfo.result != null && findInfo.result.Count == 1)
                {
                    //删除
                    request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.cloudflare.com/client/v4/zones/{cfZoom}/dns_records/{findInfo.result[0].id}");
                    request.Headers.Add("Authorization", $"Bearer {cfKey}");
                    var deleteTxt = await HttpHelper.SendAsync(request);
                    var deleteInfo = JsonHelper.JsonToObj<CFMessageInfo>(deleteTxt);
                    LogHelper.logApp.Info($"删除先前域名解析-日志:{JsonHelper.ObjToJson(deleteInfo)}");
                    LogHelper.logApp.Info($"删除先前域名解析-数据:{JsonHelper.ObjToJson(findInfo)}");
                    isDelete = true;
                }

                //添加
                var selectCDN = cdnList.Find(t => t.code.Equals(cdnCode)); 

                if (selectCDN != null)
                {
                    //添加
                    request = new HttpRequestMessage(HttpMethod.Post, $"https://api.cloudflare.com/client/v4/zones/{cfZoom}/dns_records");
                    request.Headers.Add("Authorization", $"Bearer {cfKey}");
                    CFAddMessageInfo cfAdd = new CFAddMessageInfo();
                    cfAdd.content = selectCDN.content2;
                    cfAdd.name = genericUrl;
                    cfAdd.proxied = false;
                    cfAdd.type = selectCDN.content;
                    cfAdd.comment = "自动创建解析";
                    cfAdd.ttl = 60;
                    var content = new StringContent(JsonHelper.ObjToJson(cfAdd), null, "text/plain");
                    request.Content = content;
                    var txt = await HttpHelper.SendAsync(request);
                    var obj = JsonHelper.JsonToObj<CFMessageInfo>(txt);

                    if (!obj.success)
                    {
                        //不成功就还原之前删除的
                        if (isDelete)
                        {
                            request = new HttpRequestMessage(HttpMethod.Post, $"https://api.cloudflare.com/client/v4/zones/{cfZoom}/dns_records");
                            request.Headers.Add("Authorization", $"Bearer {cfKey}");
                            cfAdd = new CFAddMessageInfo();
                            cfAdd.content = findInfo.result[0].content;
                            cfAdd.name = genericUrl;
                            cfAdd.proxied = false;
                            cfAdd.type = findInfo.result[0].type;
                            cfAdd.comment = "自动创建解析";
                            cfAdd.ttl = 60;
                            content = new StringContent(JsonHelper.ObjToJson(cfAdd), null, "text/plain");
                            request.Content = content;
                            txt = await HttpHelper.SendAsync(request);
                            obj = JsonHelper.JsonToObj<CFMessageInfo>(txt);
                        }
                        throw new ServiceException($"添加失败!{JsonHelper.ObjToJson(obj)}");
                    }
                    else
                    {
                        //更新所有ns的cdn
                        await Dal.Db.Updateable<Nightscout>().SetColumns(t => t.cdn, cdnCode).Where(t => t.cdn == defaultCDN.content).ExecuteCommandAsync();
                        defaultCDN.content = cdnCode;
                        await _dicService.Dal.Update(defaultCDN); 
                    }
                }
                else
                {
                    throw new ServiceException("未找到对应的CDN!");
                }
            }
            else
            {
                throw new ServiceException("无需修改!");
            }

        }
    
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

            DicType defaultCDN = await _dicService.GetDic(DicTypeList.defaultCDN);
            var cdnList = await _dicService.GetDicData(CDNList.KEY);

            var cdnKey = await _dicService.GetDicData(CDNKey.KEY);
            var cfZoom = cdnKey.Find(t => CDNKey.zoomID.Equals(t.code)).content;
            var cfKey = cdnKey.Find(t => CDNKey.key.Equals(t.code)).content;

            if (!defaultCDN.content.Equals(nightscout.cdn))
            {
                //非默认cdn添加解析 

                var selectCDN = cdnList.Find(t => t.code.Equals(nightscout.cdn));
                if(selectCDN != null)
                {
                    //添加
                    var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.cloudflare.com/client/v4/zones/{cfZoom}/dns_records");
                    request.Headers.Add("Authorization", $"Bearer {cfKey}");
                    CFAddMessageInfo cfAdd = new CFAddMessageInfo();
                    cfAdd.content = selectCDN.content2;
                    cfAdd.name = nightscout.url;
                    cfAdd.proxied = false;
                    cfAdd.type = selectCDN.content;
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

            var cdnKey = await _dicService.GetDicData(CDNKey.KEY);
            var cfZoom = cdnKey.Find(t => CDNKey.zoomID.Equals(t.code)).content;
            var cfKey = cdnKey.Find(t => CDNKey.key.Equals(t.code)).content;

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.cloudflare.com/client/v4/zones/{cfZoom}/dns_records?name={nightscout.url}");
            request.Headers.Add("Authorization", $"Bearer {cfKey}");
            var txt = await HttpHelper.SendAsync(request);
            var obj = JsonHelper.JsonToObj<CFMessageListInfo>(txt);

            if (obj.success && obj.result != null && obj.result.Count == 1)
            {
                //删除
                request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.cloudflare.com/client/v4/zones/{cfZoom}/dns_records/{obj.result[0].id}");
                request.Headers.Add("Authorization", $"Bearer {cfKey}");
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
            //var master = (await _nightscoutServerServices.Dal.Query(t => t.isMongo == true)).FirstOrDefault();
            //var master = nsserver;
            var curNsserverMongoSsh = await _nightscoutServerServices.Dal.QueryById(nsserver.mongoServerId);
            NightscoutLog log = new NightscoutLog();
            StringBuilder sb = new StringBuilder();
            try
            {
                using (var sshClient = new SshClient(curNsserverMongoSsh.serverIp, curNsserverMongoSsh.serverPort, curNsserverMongoSsh.serverLoginName, curNsserverMongoSsh.serverLoginPassword))
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
                //FileHelper.FileDel($"/etc/nginx/conf.d/nightscout/{nightscout.Id}.conf");

                using (var sshClient = new SshClient(nsserver.serverIp, nsserver.serverPort, nsserver.serverLoginName, nsserver.serverLoginPassword))
                {
                    //创建SSH
                    sshClient.Connect();
                    using (var cmd = sshClient.CreateCommand(""))
                    {
                        //刷新nginx
                        var master = await _nightscoutServerServices.Dal.QueryById(nsserver.nginxServerId);
                        if (master != null)
                        {
                            using (var sshMasterClient = new SshClient(master.serverIp, master.serverPort, master.serverLoginName, master.serverLoginPassword))
                            {
                                sshMasterClient.Connect();
                                using (var cmdMaster = sshMasterClient.CreateCommand(""))
                                {
                                    
                                    var resMaster = "";
                                    var nsNginxCatalog = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.nsNginxCatalog);
                                    var excNginx = $"rm {nsNginxCatalog.content}/{nightscout.Id}.conf -f";
                                    resMaster += cmdMaster.Execute(excNginx);

                                    resMaster +=  cmdMaster.Execute("docker exec -t nginxserver nginx -s reload");
                                    NsInfo.ngCount = NsInfo.ngCount + 1;
                                    if (NsInfo.ngCount >= 10)
                                    {
                                        NsInfo.ngCount = 0;
                                        resMaster += cmdMaster.Execute("docker exec -t nginxserver nginx -s stop");
                                    }
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
        public async Task Refresh(Nightscout nightscout, NightscoutServer nsserver)
        {
            await StopDocker(nightscout, nsserver);
            await RunDocker(nightscout, nsserver);
            nightscout.isStop = false;
            await Dal.Update(nightscout, t => new { t.isStop });
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
                            string cmdStr = await GetNsDockerConfig(nightscout, nsserver);
                            res = cmd.Execute(cmdStr);
                            sb.AppendLine($"启动实例:{res}");

                            //刷新nginx
                            //var master = (await _nightscoutServerServices.Dal.Query(t => t.isNginx == true)).FirstOrDefault();
                            var master = await _nightscoutServerServices.Dal.QueryById(nsserver.nginxServerId);
                            if (master != null)
                            {
                                using (var sshMasterClient = new SshClient(master.serverIp, master.serverPort, master.serverLoginName, master.serverLoginPassword))
                                {
                                    sshMasterClient.Connect();
                                    using (var cmdMaster = sshMasterClient.CreateCommand(""))
                                    {
                                        var resMaster = "";

                                        //FileHelper.WriteFile($"/etc/nginx/conf.d/nightscout/{nightscout.Id}.conf", webConfig);


                                        var nsNginxCatalog = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.nsNginxCatalog);
                                        var excNginx = $@"
cat > {nsNginxCatalog.content}/{nightscout.Id}.conf << 'EOF'
{webConfig}
EOF

";
                                        resMaster += cmdMaster.Execute(excNginx);
                                        resMaster += cmdMaster.Execute("docker exec -t nginxserver nginx -s reload");
                                        NsInfo.ngCount = NsInfo.ngCount + 1;
                                        if (NsInfo.ngCount >= 10)
                                        {
                                            NsInfo.ngCount = 0;
                                            resMaster += cmdMaster.Execute("docker exec -t nginxserver nginx -s stop");
                                        }
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
        public async Task<string> GetNsDockerConfig(Nightscout nightscout, NightscoutServer nsserver)
        {
            List<string> args = new List<string>();
            if (nightscout.exposedPort > 0)
            {
                //外网
                args.Add($"docker run -m {(nightscout.nsMemory > 0 ? nightscout.nsMemory : 100)}m --cpus=1 --restart=always -p {nightscout.exposedPort}:1337 --name {nightscout.serviceName}");
            }
            else
            {
                //内网
                args.Add($"docker run -m {(nightscout.nsMemory > 0 ? nightscout.nsMemory : 100)}m --cpus=1 --restart=always --net mynet --ip {nightscout.instanceIP} --name {nightscout.serviceName}");
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

            var nsInfo = await _dicService.GetDicData(NsInfo.KEY);
            if (nightscout.isConnection)
            {
                args.Add($"-e MAKER_KEY={nsInfo.Find(t=> NsInfo.MAKER_KEY.Equals(t.code)).content}");
                if (nightscout.isKeepPush)
                {
                    args.Add($"-e KEEP_PUSH='true'");
                }
                args.Add($"-e PUSH_URL='{nsInfo.Find(t => NsInfo.pushUrl.Equals(t.code)).content}'");
            }
            args.Add($"-e LANGUAGE=zh_cn");
            args.Add($"-e DISPLAY_UNITS='mmol/L'");
            args.Add($"-e TIME_FORMAT=24");
            args.Add($"-e THEME=colors");


            var nsPlugins = await _dicService.GetDicData(NSplugins.KEY);

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
                    pluginsArr = nsPlugins.Select(t => t.code).ToList();
                }
            }
            catch (Exception)
            {
                pluginsArr = nsPlugins.Select(t => t.code).ToList();
            }
            args.Add($"-e SHOW_PLUGINS='{string.Join(" ", pluginsArr)}'");
            args.Add($"-e ENABLE='{string.Join(" ", pluginsArr)}'");

            //args.Add($"-e SHOW_PLUGINS='careportal basal dbsize rawbg iob maker cob bridge bwp cage iage sage boluscalc pushover treatmentnotify mmconnect loop pump profile food openaps bage alexa override cors'");
            //args.Add($"-e ENABLE='careportal basal dbsize rawbg iob maker cob bridge bwp cage iage sage boluscalc pushover treatmentnotify mmconnect loop pump profile food openaps bage alexa override cors'");

            args.Add($"-e AUTH_DEFAULT_ROLES=readable");



            var defaultNsVersion = await _dicService.GetDic(DicTypeList.defaultNsVersion);

            var cusInfo = await _nightscoutCustomerServices.Dal.QueryById(nightscout.customerId);
            var checkUrl = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.preCheckUrl);

            //自定义环境变量
            args.Add($"-e uid={nightscout.Id}");
            args.Add($"-e CUSTOM_TITLE='{(cusInfo == null ? nsInfo.Find(t => NsInfo.CUSTOM_TITLE.Equals(t.code)).content : cusInfo.name)}'");
            args.Add($"-e name='{(cusInfo == null ? nsInfo.Find(t => NsInfo.CUSTOM_TITLE.Equals(t.code)).content : cusInfo.name)}'");
            args.Add($"-e logo='{(cusInfo == null ? "" : cusInfo.logo)}'");
            args.Add($"-e preCheckUrl='{checkUrl.content}'");

            //基础率显示默认
            args.Add($"-e BASAL_RENDER='default'");
             
            

            //苹果远程指令
            var appleRemote = await _dicService.GetDicData(AppleRemote.KEY);
            args.Add($"-e LOOP_APNS_KEY_ID='{appleRemote.Find(t => AppleRemote.apKeyID.Equals(t.code)).content}'");
            args.Add($"-e LOOP_APNS_KEY=$'{appleRemote.Find(t => AppleRemote.apKey.Equals(t.code)).content}'");
            args.Add($"-e LOOP_DEVELOPER_TEAM_ID='{appleRemote.Find(t => AppleRemote.apTeamID.Equals(t.code)).content}'");
            args.Add($"-e LOOP_PUSH_SERVER_ENVIRONMENT='{appleRemote.Find(t => AppleRemote.apEnv.Equals(t.code)).content}'");

            //ns启动版本
            //args.Add($"-d nightscout/cgm-remote-monitor:latest");
            args.Add($"-d {nsInfo.Find(t => NsInfo.image.Equals(t.code)).content}{(string.IsNullOrEmpty(nightscout.nsVersion) ? (":" + defaultNsVersion.content) : (":" + nightscout.nsVersion))}");

            var cmdStr = string.Join(" ", args);
            LogHelper.logApp.Info($"ns启动参数获取:{nightscout.name},id:{nightscout.Id}");
            LogHelper.logApp.Info(cmdStr);
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
            var nsInfo = _dicService.GetDicData(NsInfo.KEY).Result;

            return @$"
server {{
    listen 443 ssl ;
    server_name {nightscout.url} {nightscout.backupurl};

    ssl_certificate ""/etc/nginx/conf.d/{(string.IsNullOrEmpty(nightscout.sslCerName) ? nsInfo.Find(t => NsInfo.cer.Equals(t.code)).content : nightscout.sslCerName)}"";
    ssl_certificate_key ""/etc/nginx/conf.d/{(string.IsNullOrEmpty(nightscout.sslKeyName) ? nsInfo.Find(t => NsInfo.key.Equals(t.code)).content : nightscout.sslKeyName)}"";

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
        /// 删除超过期限的用户血糖数据
        /// </summary>
        /// <returns></returns>
        public async Task ClearMongoData(Nightscout nightscout, NightscoutServer nsserver)
        {

            try
            {
                //var nsInfo = await _dicService.GetDicData(NsInfo.KEY);

                var outOfDate = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.cleanOutBlood);

                //Builders<BsonDocument>.Filter.Gt("date", 0) &
                var flagTime = DateTime.Now;
                flagTime = flagTime.AddDays(-outOfDate.content.ObjToInt()).ToUniversalTime();

                //数据库链接
                var client = new MongoClient($"mongodb://{nsserver.mongoLoginName}:{nsserver.mongoLoginPassword}@{nsserver.mongoIp}:{nsserver.mongoPort}");
                //var client = new MongoClient($"mongodb://{NsInfo.miniLoginName}:{NsInfo.miniLoginPasswd}@{NsInfo.miniHost}:{NsInfo.miniPort}"); // 连接到MongoDB
                var database = client.GetDatabase(nightscout.serviceName); // 获取数据库对象



                var collectionEntries = database.GetCollection<BsonDocument>("entries"); // 替换为你的集合名称
                var filterEntries = Builders<BsonDocument>.Filter.Lte("date", (flagTime - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);
                //var ls = await collectionEntries.Find(filterEntries).ToListAsync();
                var resEntries = collectionEntries.DeleteMany(filterEntries); // 删除匹配的数据
                                                             

                var collectionDevicestatus = database.GetCollection<BsonDocument>("devicestatus"); // 替换为你的集合名称
                var filterDevicestatus = Builders<BsonDocument>.Filter.Lte("created_at", flagTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                //var ls = await collectionDevicestatus.Find(filterDevicestatus).ToListAsync();
                var resDevicestatus = collectionDevicestatus.DeleteMany(filterDevicestatus); // 删除匹配的数据

                var collectionTreatments = database.GetCollection<BsonDocument>("treatments"); // 替换为你的集合名称
                var filterTreatments = Builders<BsonDocument>.Filter.Lte("created_at", flagTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                //var ls = await collectionTreatments.Find(filterTreatments).ToListAsync();
                var resTreatments = collectionTreatments.DeleteMany(filterTreatments); // 删除匹配的数据


                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogHelper.logApp.Error($"ns用户({nightscout.Id})数据删除失败:{ex.Message}", ex);
            }


        }
        /// <summary>
        /// 停止长期未使用的ns实例
        /// </summary>
        /// <param name="nightscout"></param>
        /// <param name="nsserver"></param>
        /// <returns></returns>

        public async Task StopLongTimeNoUseNs(Nightscout nightscout, NightscoutServer nsserver)
        {

            try
            {
                var stopOfDate = await _dicService.GetDicDataOne(NsInfo.KEY, NsInfo.stopOutBlood);
                int outDays = stopOfDate.content.ObjToInt();
                //数据库链接
                var client = new MongoClient($"mongodb://{nsserver.mongoLoginName}:{nsserver.mongoLoginPassword}@{nsserver.mongoIp}:{nsserver.mongoPort}");
                var database = client.GetDatabase(nightscout.serviceName); // 获取数据库对象 

                var collectionEntries = database.GetCollection<BsonDocument>("entries"); 

                // 创建过滤条件，排序方式为倒序
                var filter = Builders<BsonDocument>.Filter.Empty;
                var sort = Builders<BsonDocument>.Sort.Descending("date");
                var projection = Builders<BsonDocument>.Projection.Include("date").Include("sgv").Include("dateString").Exclude("_id");
                // 查询并获取第一条数据
                var result = await collectionEntries.Find(filter).Sort(sort).Limit(1).Project(projection).ToListAsync();
                var sugers = JsonHelper.JsonToObj<List<entries>>(result.ToJson()).FirstOrDefault();
                if (sugers == null)
                {
                    //没有数据的情况
                    var lastDate = nightscout.lastUpdateTime == null ? nightscout.startTime : nightscout.lastUpdateTime.Value;
                    lastDate = lastDate.ToUniversalTime();
                    //超过期限停止实例
                    if ((DateTime.Now - lastDate).TotalDays >= outDays)
                    {
                        LogHelper.logApp.Info($"停止实例id:{nightscout.Id},名称:{nightscout.name}");
                        await StopDocker(nightscout, nsserver);
                        await Dal.Db.Updateable<Nightscout>().SetColumns(t => t.isStop, true).Where(t => t.Id == nightscout.Id).ExecuteCommandAsync();
                    }
                }
                else
                {
                    //有数据的情况

                    var lastUpdateTime = sugers.dateString.ObjToDate();
                    //超过期限停止实例
                    if ((DateTime.Now - lastUpdateTime).TotalDays >= outDays)
                    {
                        LogHelper.logApp.Info($"停止实例id:{nightscout.Id},名称:{nightscout.name}");
                        await StopDocker(nightscout, nsserver);
                        await Dal.Db.Updateable<Nightscout>().SetColumns(t => t.isStop, true).Where(t => t.Id == nightscout.Id).ExecuteCommandAsync();
                    }
                    await Dal.Db.Updateable<Nightscout>().SetColumns(t => t.lastUpdateTime, lastUpdateTime).Where(t => t.Id == nightscout.Id).ExecuteCommandAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.logApp.Error($"{nightscout.name},id:{nightscout.Id},实例停止异常", ex);
            }
            await Task.CompletedTask;
        }

        
    }

    
}