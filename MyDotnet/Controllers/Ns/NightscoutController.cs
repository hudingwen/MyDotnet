using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Dto.WeChat;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.WeChat;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using MyDotnet.Services.Ns;
using MyDotnet.Services.WeChat;
using SqlSugar;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MyDotnet.Controllers.Ns
{
    /// <summary>
    /// Nightscout控制器
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class NightscoutController : Controller
    {

        public NightscoutServices _nightscoutServices;

        public WeChatConfigServices _weChatConfigServices;

        public BaseServices<NightscoutLog> _nightscoutLogServices;

        public BaseServices<NightscoutServer> _nightscoutServerServices;

        public BaseServices<WeChatSub> _wechatsubServices;

        public UnitOfWorkManage _unitOfWorkManage;

        public NightscoutController(NightscoutServices nightscoutServices
            , WeChatConfigServices weChatConfigServices
            , BaseServices<NightscoutLog> nightscoutLogServices
            , BaseServices<NightscoutServer> nightscoutServerServices
            , BaseServices<WeChatSub> wechatsubServices
            , UnitOfWorkManage unitOfWorkManage
            )
        {
            _nightscoutServices = nightscoutServices;
            _weChatConfigServices = weChatConfigServices;
            _nightscoutLogServices = nightscoutLogServices;
            _nightscoutServerServices = nightscoutServerServices;
            _wechatsubServices = wechatsubServices;
            _unitOfWorkManage = unitOfWorkManage;
        }


        /// <summary>
        /// 添加国内解析
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> ResolveDomain(long id)
        {
            var nightscout = await _nightscoutServices.Dal.QueryById(id);
            var isSuccess = await _nightscoutServices.ResolveDomain(nightscout);
            if (isSuccess)
            {
                return MessageModel<string>.Success("添加国内解析成功");
            }
            else
            {
                return MessageModel<string>.Fail("添加国内解析失败");
            }
        }
        /// <summary>
        /// 删除国内解析
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> UnResolveDomain(long id)
        {
            var nightscout = await _nightscoutServices.Dal.QueryById(id);
            var isSuccess = await _nightscoutServices.UnResolveDomain(nightscout);
            if (isSuccess)
            {
                return MessageModel<string>.Success("删除国内解析成功");
            }
            else
            {
                return MessageModel<string>.Fail("删除国内解析失败");
            }
        }

        [HttpGet]
        public async Task<MessageModel<PageModel<Nightscout>>> Get(int page = 1, string key = "", int pageSize = 50)
        {


            Expression<Func<Nightscout, bool>> whereExpression = a => true;

            if (!(string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key)))
            {
                key = key.Trim();
                whereExpression = whereExpression.And(t => t.name.Contains(key) || t.url.Contains(key) || t.passwd.Contains(key) || t.status.Contains(key) || t.resource.Contains(key));
            }
            var data = await _nightscoutServices.Dal.QueryPage(whereExpression, page, pageSize);


            var bindData = await _wechatsubServices.Dal.Query(t => t.SubFromPublicAccount == NsInfo.pushWechatID && t.IsUnBind == false && t.CompanyID == NsInfo.pushCompanyCode && data.data.Select(i => i.Id.ToString()).ToList().Contains(t.SubJobID));
            //isBindWeChat
            foreach (var item in data.data)
            {
                var find = bindData.Find(t => t.SubJobID.Equals(item.Id.ToString()));
                if (find != null)
                {
                    item.isBindWeChat = true;
                }
            }


            var bindMini = await _wechatsubServices.Dal.Query(t => t.SubFromPublicAccount == NsInfo.miniAppid && t.IsUnBind == false && t.CompanyID == NsInfo.pushCompanyCode && data.data.Select(i => i.Id.ToString()).ToList().Contains(t.SubJobID));
            //isBindWeChat
            foreach (var item in data.data)
            {
                var find = bindMini.Find(t => t.SubJobID.Equals(item.Id.ToString()));
                if (find != null)
                {
                    item.isBindMini = true;
                }
            }
            return new MessageModel<PageModel<Nightscout>>()
            {
                msg = "获取成功",
                success = true,
                response = data
            };

        }


        [HttpGet("{id}")]
        public async Task<MessageModel<Nightscout>> Get(long id)
        {
            return new MessageModel<Nightscout>()
            {
                msg = "获取成功",
                success = true,
                response = await _nightscoutServices.Dal.QueryById(id)
            };
        }
        private void FilterData(Nightscout request)
        {
            request.name = request.name.ObjToString().Trim();
            request.url = request.url.ObjToString().Trim();
            request.passwd = request.passwd.ObjToString().Trim();
            request.tel = request.tel.ObjToString().Trim();
            request.instanceIP = request.instanceIP.ObjToString().Trim();
            request.serviceName = request.serviceName.ObjToString().Trim();
            if (string.IsNullOrEmpty(request.resource)) request.resource = "未确认";
            if (string.IsNullOrEmpty(request.status)) request.resource = "未启用";
        }
        private async Task<MessageModel<string>> CheckData(Nightscout request)
        {
            if (string.IsNullOrEmpty(request.name))
                return MessageModel<string>.Fail($"名称不能为空");

            if (string.IsNullOrEmpty(request.passwd))
                return MessageModel<string>.Fail($"密码不能为空");

            if (string.IsNullOrEmpty(request.url))
                return MessageModel<string>.Fail($"域名不能为空");

            Expression<Func<Nightscout, bool>> whereExpression = a => true;

            whereExpression = whereExpression.And(t => t.Id != request.Id);
            whereExpression = whereExpression.And(t => t.url == request.url);
            var data = await _nightscoutServices.Dal.Query(whereExpression);
            if (data != null && data.Count > 0)
                return MessageModel<string>.Fail($"当前操作的[{request.name}]与[{data[0].name}]的网址配置有冲突,请检查");

            whereExpression.And(t => t.Id != request.Id);
            whereExpression = whereExpression.And(t => t.exposedPort == 0 && t.instanceIP == request.instanceIP && t.serverId == request.serverId);
            data = await _nightscoutServices.Dal.Query(whereExpression);
            if (data != null && data.Count > 0)
                return MessageModel<string>.Fail($"当前操作的[{request.name}]与[{data[0].name}]的IP配置有冲突,请检查");

            whereExpression.And(t => t.Id != request.Id);
            whereExpression = whereExpression.And(t => t.exposedPort > 0 && t.exposedPort == request.exposedPort && t.serverId == request.serverId);
            data = await _nightscoutServices.Dal.Query(whereExpression);
            if (data != null && data.Count > 0)
                return MessageModel<string>.Fail($"当前操作的[{request.name}]与[{data[0].name}]的端口配置有冲突,请检查");

            whereExpression.And(t => t.Id != request.Id);
            whereExpression = whereExpression.And(t => t.serviceName == request.serviceName);
            data = await _nightscoutServices.Dal.Query(whereExpression);
            if (data != null && data.Count > 0)
                return MessageModel<string>.Fail($"当前操作的[{request.name}]与[{data[0].name}]的服务名称配置有冲突,请检查");


            return MessageModel<string>.Success("");

        }
        /// <summary>
        /// 随机生成几位数
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private string GenerateNumber(int count)
        {
            Random random = new Random();
            string r = "";
            int i;
            for (i = 1; i <= count; i++)
            {
                r += random.Next(0, 9).ToString();
            }
            return r;
        }
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Nightscout request)
        {
            FilterData(request);

            var data = new MessageModel<string>();

            if (request.serverId > 0)
            {
                var nsserver = await _nightscoutServerServices.Dal.QueryById(request.serverId);
                if (request.serviceSerial == 0)
                {
                    //新增实例

                    //设置服务名称
                    nsserver.curServiceSerial += 1;
                    request.serviceName = $"nightscout-template{nsserver.curServiceSerial}";
                    request.serviceSerial = nsserver.curServiceSerial;

                    //设置访问IP
                    if (nsserver.curExposedPort > 0)
                    {
                        //通过暴露端口访问
                        nsserver.curExposedPort += 1;
                        request.exposedPort = nsserver.curExposedPort;
                        request.instanceIP = nsserver.serverIp;
                    }
                    else
                    {
                        //通过实例DockerIP访问
                        nsserver.curInstanceIpSerial += 1;
                        //192.168.0.{}
                        nsserver.curInstanceIp = string.Format(nsserver.instanceIpTemplate, nsserver.curInstanceIpSerial);
                        request.instanceIP = nsserver.curInstanceIp;
                    }
                    //不填写自动生成
                    var padName = nsserver.curServiceSerial.ObjToString().PadLeft(3, '0');
                    if (string.IsNullOrEmpty(request.name))
                    {
                        request.name = padName;
                    }
                    if (string.IsNullOrEmpty(request.passwd))
                    {
                        request.passwd = GenerateNumber(5);
                    }
                    if (string.IsNullOrEmpty(request.url))
                    {
                        request.url = string.Format(NsInfo.templateUrl, GenerateNumber(3) + padName);
                    }




                }

                var check = await CheckData(request);
                if (!check.success) return check;


                try
                {
                    //开启事务
                    _unitOfWorkManage.BeginTran();
                    var id = await _nightscoutServices.Dal.Add(request);
                    request.Id = id;
                    await _nightscoutServerServices.Dal.Update(nsserver);
                    await _nightscoutServerServices.Dal.Db.Updateable<NightscoutServer>().SetColumns("curServiceSerial", nsserver.curServiceSerial).Where(t => t.Id > 0).ExecuteCommandAsync();
                    _unitOfWorkManage.CommitTran();

                    data.success = id > 0;
                    if (data.success)
                    {
                        request.Id = id;
                        data.response = id.ObjToString();
                        data.msg = "添加成功";
                        //第一次默认就启动
                        await _nightscoutServices.InitData(request, nsserver);
                        await _nightscoutServices.RunDocker(request, nsserver);
                    }
                    else
                    {
                        data.msg = "添加失败";
                    }
                }
                catch (Exception)
                {
                    _unitOfWorkManage.RollbackTran();
                    throw;
                }
            }
            else
            {
                return MessageModel<string>.Fail("请选选择一个服务器");
            }
            return data;
        }

        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Nightscout request)
        {
            FilterData(request);
            var check = await CheckData(request);
            if (!check.success) return check;
            var data = new MessageModel<string>();
            var old = await _nightscoutServices.Dal.QueryById(request.Id);

            data.success = await _nightscoutServices.Dal.Update(request);
            if (data.success)
            {
                data.msg = "更新成功";
                data.response = request?.Id.ObjToString();
            }
            bool isDiff = false;
            if (!request.url.Equals(old.url))
            {
                //域名切换删除加速解析
                await _nightscoutServices.UnResolveDomain(old);
            }
            if (!request.serverId.Equals(old.serverId))
            {
                //不是同一个服务器需要停掉先前服务器

                var oldNsserver = await _nightscoutServerServices.Dal.QueryById(old.serverId);
                await _nightscoutServices.StopDocker(old, oldNsserver);

                var nsserver = await _nightscoutServerServices.Dal.QueryById(request.serverId);
                if (nsserver.curExposedPort > 0)
                {
                    //通过暴露端口访问
                    nsserver.curExposedPort += 1;
                    request.exposedPort = nsserver.curExposedPort;
                    request.instanceIP = nsserver.serverIp;
                }
                else
                {
                    //通过实例DockerIP访问
                    nsserver.curInstanceIpSerial += 1;
                    //192.168.0.{}
                    nsserver.curInstanceIp = string.Format(nsserver.instanceIpTemplate, nsserver.curInstanceIpSerial);
                    request.instanceIP = nsserver.curInstanceIp;
                }
                _unitOfWorkManage.BeginTran();
                try
                {
                    await _nightscoutServerServices.Dal.Update(nsserver);
                    data.success = await _nightscoutServices.Dal.Update(request);
                    _unitOfWorkManage.CommitTran();
                }
                catch (Exception)
                {
                    _unitOfWorkManage.RollbackTran();
                    throw;
                }
                await _nightscoutServices.RunDocker(request, nsserver);
                isDiff = true;
            }
            if (request.isRefresh && !isDiff)
            {
                var nsserver = await _nightscoutServerServices.Dal.QueryById(request.serverId);
                await _nightscoutServices.StopDocker(request, nsserver);
                await _nightscoutServices.RunDocker(request, nsserver);
            }
            return data;
        }

        [HttpDelete]
        public async Task<MessageModel<string>> Delete(long id)
        {
            var data = new MessageModel<string>();
            var model = await _nightscoutServices.Dal.QueryById(id);
            model.IsDeleted = true;
            data.success = await _nightscoutServices.Dal.Update(model);
            if (data.success)
            {
                data.msg = "删除成功";
                data.response = model?.Id.ObjToString();
            }
            var nsserver = await _nightscoutServerServices.Dal.QueryById(model.serverId);
            await _nightscoutServices.StopDocker(model, nsserver);
            await _nightscoutServices.DeleteData(model, nsserver);
            return data;
        }
        /// <summary>
        /// 重置数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> Reset(long id)
        {
            var data = await _nightscoutServices.Dal.QueryById(id);
            if (data == null || data.IsDeleted) return MessageModel<string>.Fail("实例不存在");
            var nsserver = await _nightscoutServerServices.Dal.QueryById(data.serverId);
            await _nightscoutServices.StopDocker(data, nsserver);

            await _nightscoutServices.DeleteData(data, nsserver);
            await _nightscoutServices.InitData(data, nsserver);

            await _nightscoutServices.RunDocker(data, nsserver);
            return MessageModel<string>.Success("刷新成功");
        }
        /// <summary>
        /// 停止实例
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> Stop(long id)
        {
            var nightscout = await _nightscoutServices.Dal.QueryById(id);
            if (nightscout == null || nightscout.IsDeleted) return MessageModel<string>.Fail("实例不存在");
            var nsserver = await _nightscoutServerServices.Dal.QueryById(nightscout.serverId);
            await _nightscoutServices.StopDocker(nightscout, nsserver);
            nightscout.isStop = true;
            await _nightscoutServices.Dal.Update(nightscout, t => new { t.isStop });
            return MessageModel<string>.Success("停止成功");
        }
        /// <summary>
        /// 刷新实例
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> Refresh(long id)
        {
            var data = await _nightscoutServices.Dal.QueryById(id);
            if (data == null || data.IsDeleted) return MessageModel<string>.Fail("实例不存在");
            var nsserver = await _nightscoutServerServices.Dal.QueryById(data.serverId);
            await _nightscoutServices.StopDocker(data, nsserver);
            await _nightscoutServices.RunDocker(data, nsserver);
            return MessageModel<string>.Success("刷新成功");
        }
        /// <summary>
        /// nightscout微信绑定二维码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<WeChatResponseUserInfo>> GetWeChatCode(long id)
        {
            var data = await _nightscoutServices.Dal.QueryById(id);
            if (!data.isConnection) return MessageModel<WeChatResponseUserInfo>.Fail("实例还未接入微信");
            if (data == null || data.IsDeleted) return MessageModel<WeChatResponseUserInfo>.Fail("实例不存在");
            return await _weChatConfigServices.GetQRBind(new WeChatUserInfo { userID = id.ToString(), companyCode = NsInfo.pushCompanyCode, id = NsInfo.pushWechatID, userNick = data.name });
        }
        /// <summary>
        /// nightscout取消绑定
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<WeChatResponseUserInfo>> UnbindWeChat(long id)
        {
            var data = await _nightscoutServices.Dal.QueryById(id);
            if (data == null || data.IsDeleted) return MessageModel<WeChatResponseUserInfo>.Fail("实例不存在");
            return await _weChatConfigServices.UnBind(new WeChatUserInfo { userID = id.ToString(), companyCode = NsInfo.pushCompanyCode, id = NsInfo.pushWechatID });
        }

        /// <summary>
        /// 获取实例运行日志
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<NightscoutLog>>> GetLog(long id, int page = 1, int pageSize = 50)
        {

            var data = await _nightscoutServices.Dal.QueryById(id);
            if (data == null || data.IsDeleted) return MessageModel<PageModel<NightscoutLog>>.Fail("实例不存在");
            Expression<Func<NightscoutLog, bool>> whereExpression = a => true;
            whereExpression = whereExpression.And(t => t.pid == id);
            return new MessageModel<PageModel<NightscoutLog>>()
            {
                msg = "获取成功",
                success = true,
                response = await _nightscoutLogServices.Dal.QueryPage(whereExpression, page, pageSize, "Id desc")
            };
        }
        /// <summary>
        /// 推送血糖
        /// </summary>
        /// <param name="data"></param>
        [HttpGet]
        [AllowAnonymous]
        public async Task<string> Push([FromQuery] BloodInfo data)
        {
            //LogHelper.Info($"进入nightscout");
            //LogHelper.Info($"nightscout原始数据:{JsonConvert.SerializeObject(data)}");
            if ("bwp".Equals(data.Value3))
            {
                return "bwp跳过";
            }
            if ("ns-allclear".Equals(data.Value5))
            {
                return "All Clear跳过";
            }

            var pushTemplateID = string.Empty;
            var pushData = new WeChatCardMsgDataDto();
            pushData.cardMsg = new WeChatCardMsgDetailDto();
            Nightscout nightscout;

            if ("ns-keep".Equals(data.Value5))
            {
                nightscout = await _nightscoutServices.Dal.QueryById(data.Value4);
                if (nightscout == null || nightscout.IsDeleted)
                {
                    //LogHelper.Info("nightscout用户未找到");
                    return "nightscout用户未找到";
                }
                pushTemplateID = NsInfo.pushTemplateID_Keep;
                data.Value1 = "血糖持续监测";
                data.Value2_1 = data.Value2;


                pushData.cardMsg.first = $"您好！{nightscout.name}，为您推送实时血糖监测";
                pushData.cardMsg.keyword1 = $"实时血糖：{data.Value2_1}";
                pushData.cardMsg.keyword2 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else if ("ns-event".Equals(data.Value5))
            {
                nightscout = await _nightscoutServices.Dal.QueryById(data.Value4);
                if (nightscout == null || nightscout.IsDeleted)
                {
                    //LogHelper.Info("nightscout用户未找到");
                    return "nightscout用户未找到";
                }
                pushTemplateID = NsInfo.pushTemplateID_Exception;
                data.Value1 = data.Value1.Replace(",", " -");
                var ls = data.Value2.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                if (ls.Length > 1)
                {
                    data.Value2_1 = ls[0].Replace("BG Now: ", "");
                    data.Value2_2 = ls[1].Replace("BG 15m: ", "");
                }
                pushData.cardMsg.first = $"您好！{nightscout.name}，当前血糖异常";
                pushData.cardMsg.keyword1 = data.Value2_1;
                pushData.cardMsg.keyword2 = data.Value1;
                pushData.cardMsg.remark = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                //LogHelper.Info("暂时跳过其他情况");
                return "暂时跳过其他情况";
            }

            pushData.cardMsg.url = $"https://{nightscout.url}";
            pushData.cardMsg.template_id = pushTemplateID;
            pushData.cardMsg.miniprogram = new WeChatCardMsgMiniprogram { appid = NsInfo.miniAppid, pagepath = NsInfo.miniPath };

            pushData.info = new WeChatUserInfo();
            pushData.info.id = NsInfo.pushWechatID;
            pushData.info.companyCode = NsInfo.pushCompanyCode;
            pushData.info.userID = nightscout.Id.ToString();

            await _weChatConfigServices.PushCardMsg(pushData);
            return "推送ns成功";
            //if (nightscout.isBindWeChat)
            //{

            //}
            //else
            //{
            //    return "用户取消关注";
            //}
        }
        [HttpGet]
        public async Task<MessageModel<object>> GetSummary()
        {
            var status = await _nightscoutServices.Dal.Db.Queryable<Nightscout>()
             .GroupBy(it => it.status)
             .Select(it => new
             {
                 count = SqlFunc.AggregateCount(it.Id),
                 name = it.status
             })
             .ToListAsync();
            var resource = await _nightscoutServices.Dal.Db.Queryable<Nightscout>()
             .GroupBy(it => it.resource)
             .Select(it => new
             {
                 count = SqlFunc.AggregateCount(it.Id),
                 name = it.resource
             })
             .ToListAsync();

            return MessageModel<object>.Success("获取成功", new { status, resource });
        }
        [HttpGet]
        public MessageModel<List<NSPlugin>> GetPlugins()
        {
            return MessageModel<List<NSPlugin>>.Success("成功", NsInfo.plugins);
        }



        [HttpGet]
        public async Task<MessageModel<PageModel<NightscoutServer>>> getNsServer(int page = 1, string key = "", int pageSize = 50)
        {
            Expression<Func<NightscoutServer, bool>> whereExpression = a => true;

            if (!(string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key)))
            {
                key = key.Trim();
                whereExpression = whereExpression.And(t => t.serverName.Contains(key));
            }
            var data = await _nightscoutServerServices.Dal.QueryPage(whereExpression, page, pageSize);
            return MessageModel<PageModel<NightscoutServer>>.Success("获取成功", data);
        }
        [HttpDelete]
        public async Task<MessageModel<string>> delNsServer(long id)
        {
            var data = new MessageModel<string>();
            var model = await _nightscoutServerServices.Dal.QueryById(id);
            model.IsDeleted = true;
            data.success = await _nightscoutServerServices.Dal.Update(model);
            if (data.success)
            {
                data.msg = "删除成功";
                data.response = model?.Id.ObjToString();
            }
            return data;
        }
        [HttpPut]
        public async Task<MessageModel<string>> updateNsServer([FromBody] NightscoutServer request)
        {
            var data = await _nightscoutServerServices.Dal.QueryById(request.Id);
            if (data == null) MessageModel<string>.Fail("要更新的数据不存在");
            var id = await _nightscoutServerServices.Dal.Update(request);
            return MessageModel<string>.Success("添加成功", id.ObjToString());
        }
        [HttpPost]
        public async Task<MessageModel<string>> addNsServer([FromBody] NightscoutServer request)
        {
            var id = await _nightscoutServerServices.Dal.Add(request);
            return MessageModel<string>.Success("添加成功", id.ObjToString());
        }
        [HttpGet]
        public async Task<MessageModel<List<NightscoutServer>>> getAllNsServer()
        {
            var data = await _nightscoutServerServices.Dal.Db.Queryable<NightscoutServer>().Select(t => new NightscoutServer { Id = t.Id, serverName = t.serverName }).ToListAsync();

            var ids = data.Select(tt => tt.Id).ToList();
            var ls = await _nightscoutServerServices.Dal.Db.Queryable<Nightscout>()
            .GroupBy(t => t.serverId)
                .Where(t => ids.Contains(t.serverId)).Select(t => new { t.serverId, count = SqlFunc.AggregateCount(t.serverId) }).ToListAsync();

            foreach (var item in data)
            {
                var row = ls.Find(t => t.serverId == item.Id);
                if (row == null)
                    item.count = 0;
                else
                    item.count = row.count;
            }
            return MessageModel<List<NightscoutServer>>.Success("获取成功", data);
        }


        /// <summary>
        /// 获取小程序绑定二维码
        /// </summary>
        /// <param name="nsid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> GetBindQR(long nsid)
        {
            string url = $"https://api.weixin.qq.com/cgi-bin/stable_token";

            var weChatToken = new { appid = NsInfo.miniAppid, secret = NsInfo.miniSecret, grant_type = NsInfo.miniGrantType };

            var nsInfo = await _nightscoutServices.Dal.QueryById(nsid);



            WeChatQR weChatQR = new WeChatQR
            {
                QRbindCompanyID = NsInfo.pushCompanyCode,
                QRbindJobID = nsInfo.Id.ObjToString(),
                QRbindJobNick = nsInfo.name,
                QRcrateTime = DateTime.Now,
                QRpublicAccount = NsInfo.miniAppid,
                QRticket = StringHelper.GetGUID()
            };

            await _weChatConfigServices.Dal.Db.Insertable<WeChatQR>(weChatQR).ExecuteCommandAsync();


            HttpContent httpContent = new StringContent(JsonHelper.ObjToJson(weChatToken));



            string result = await HttpHelper.PostAsync(url, httpContent);
            AccessTokenDto accessTokenDto = JsonHelper.JsonToObj<AccessTokenDto>(result);

            //正式版为 "release"，体验版为 "trial"，开发版为 "develop"
            var ticket = weChatQR.QRticket;
            var jsonBind = JsonHelper.ObjToJson(new { path = $"pages/index/index?ticket={ticket}", env_version = NsInfo.miniEnv, width = 128 });
            HttpContent httpContentBind = new StringContent(jsonBind);
            var urlBind = $"https://api.weixin.qq.com/wxa/getwxacode?access_token={accessTokenDto.access_token}";

            var bindstream = await HttpHelper.PostAsync(urlBind, httpContentBind);
            //return File(bindstream, "image/jpeg");
            return MessageModel<string>.Success("成功", bindstream);

        }
        /// <summary>
        /// 绑定小程序
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="openid"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<string>> BindQR(string ticket, string openid)
        {

            var nsInfo = await _weChatConfigServices.Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == NsInfo.miniAppid && t.CompanyID == NsInfo.pushCompanyCode && t.SubUserOpenID == openid).FirstAsync();
            //.Select(t => t.SubJobID)
            // && t.IsUnBind == false
            //if (!string.IsNullOrEmpty(nsid))
            if (nsInfo != null && nsInfo.IsUnBind == false)
            {
                var tempInfo = await _weChatConfigServices.Dal.Db.Queryable<WeChatQR>().Where(t => t.QRpublicAccount == NsInfo.miniAppid && t.QRticket == ticket && t.QRisUsed == false).FirstAsync();
                if (tempInfo != null)
                {
                    tempInfo.QRisUsed = true;
                    tempInfo.QRuseTime = DateTime.Now;
                    tempInfo.QRuseOpenid = openid;
                    await _weChatConfigServices.Dal.Db.Updateable<WeChatQR>(tempInfo).UpdateColumns(t => new { t.QRisUsed, t.QRuseOpenid, t.QRuseTime }).ExecuteCommandAsync();
                }

                return MessageModel<string>.Success("已绑定");
            }


            var ticketInfo = await _weChatConfigServices.Dal.Db.Queryable<WeChatQR>().Where(t => t.QRpublicAccount == NsInfo.miniAppid && t.QRticket == ticket && t.QRisUsed == false).FirstAsync();
            if (ticketInfo == null)
                return MessageModel<string>.Fail($"无效的绑定信息,请勿重复使用");
            var bindInfo = await _weChatConfigServices.Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == NsInfo.miniAppid && t.CompanyID == NsInfo.pushCompanyCode && t.SubJobID == ticketInfo.QRbindJobID).FirstAsync();

            _unitOfWorkManage.BeginTran();
            try
            {
                if (bindInfo == null)
                {
                    bindInfo = new WeChatSub
                    {
                        SubFromPublicAccount = NsInfo.miniAppid,
                        SubJobID = ticketInfo.QRbindJobID,
                        SubUserOpenID = openid,
                        CompanyID = ticketInfo.QRbindCompanyID,
                        SubUserRegTime = DateTime.Now,
                        IsUnBind = false
                    };
                    await _weChatConfigServices.Dal.Db.Insertable<WeChatSub>(bindInfo).ExecuteCommandAsync();
                }
                else
                {

                    bindInfo.LastSubUserOpenID = bindInfo.SubUserOpenID;
                    bindInfo.SubUserOpenID = openid;
                    bindInfo.SubUserRefTime = DateTime.Now;
                    bindInfo.IsUnBind = false;
                    await _weChatConfigServices.Dal.Db.Updateable<WeChatSub>(bindInfo).UpdateColumns(t => new { t.LastSubUserOpenID, t.SubUserOpenID, t.SubUserRefTime, t.IsUnBind }).ExecuteCommandAsync();
                }
                ticketInfo.QRisUsed = true;
                ticketInfo.QRuseOpenid = openid;
                ticketInfo.QRuseTime = DateTime.Now;
                await _weChatConfigServices.Dal.Db.Updateable<WeChatQR>(ticketInfo).UpdateColumns(t => new { t.QRisUsed, t.QRuseOpenid, t.QRuseTime }).ExecuteCommandAsync();
                _unitOfWorkManage.CommitTran();
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
            return MessageModel<string>.Success("绑定成功");
        }
        /// <summary>
        /// 取消绑定小程序
        /// </summary>
        /// <param name="nsid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> UnBindQR(string nsid)
        {
            var bindInfo = await _weChatConfigServices.Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == NsInfo.miniAppid && t.SubJobID == nsid && t.CompanyID == NsInfo.pushCompanyCode && t.IsUnBind == false).ToListAsync();
            if (bindInfo == null || bindInfo.Count == 0)
                return MessageModel<string>.Fail("没有找到解绑信息");
            foreach (var item in bindInfo)
            {
                item.IsUnBind = true;
                item.SubUserRefTime = DateTime.Now;
            }
            await _weChatConfigServices.Dal.Db.Updateable<WeChatSub>(bindInfo).UpdateColumns(t => new { t.IsUnBind, t.SubUserRefTime }).ExecuteCommandAsync();
            return MessageModel<string>.Success("解绑成功");
        }
        /// <summary>
        /// 小程code换openid
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatModel>> CodeLogin(string code)
        {

            var res = await HttpHelper.GetAsync($"https://api.weixin.qq.com/sns/jscode2session?appid={NsInfo.miniAppid}&secret={NsInfo.miniSecret}&js_code={code}&grant_type=authorization_code");

            var data = JsonHelper.JsonToObj<WeChatModel>(res);
            if (data.errcode.Equals(0))
            {
                return MessageModel<WeChatModel>.Success(string.Empty, data);
            }
            else
            {
                return MessageModel<WeChatModel>.Fail(string.Empty, data);
            }
        }
        /// <summary>
        /// 小程序获取血糖信息
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>

        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<SugarDTO>> GetCurBloodSugar(string openid)
        {
            SugarDTO sugarDTO = new SugarDTO();


            var nsid = await _weChatConfigServices.Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == NsInfo.miniAppid && t.CompanyID == NsInfo.pushCompanyCode && t.SubUserOpenID == openid && t.IsUnBind == false).Select(t => t.SubJobID).FirstAsync();

            if (string.IsNullOrEmpty(nsid))
                return MessageModel<SugarDTO>.Fail("无绑定信息,无法查看血糖");
            var longNsid = nsid.ObjToLong();
            var serviceName = await _weChatConfigServices.Dal.Db.Queryable<Nightscout>().Where(t => t.Id == longNsid).Select(t => t.serviceName).FirstAsync();
            if (string.IsNullOrEmpty(serviceName))
                return MessageModel<SugarDTO>.Fail("无血糖信息可供查看,请检查是否绑定NS");

            var grantConnectionMongoString = $"mongodb://{NsInfo.miniLoginName}:{NsInfo.miniLoginPasswd}@{NsInfo.miniHost}:{NsInfo.miniPort}";
            var client = new MongoClient(grantConnectionMongoString);
            var database = client.GetDatabase(serviceName);
            var collection = database.GetCollection<BsonDocument>("entries"); // 替换为你的集合名称

            //var filter = Builders<MongoDB.Bson.BsonDocument>.Filter.Empty; // 获取所有数据.Eq("name", "John");
            var filter = Builders<BsonDocument>.Filter.Gt("sgv", 0);
            var projection = Builders<BsonDocument>.Projection.Include("date").Include("sgv").Include("direction").Exclude("_id");


            var ls = await collection.Find(filter).Sort(Builders<BsonDocument>.Sort.Descending("_id")).Limit(900).Project(projection).ToListAsync();


            var sugers = JsonHelper.JsonToObj<List<EntriesEntity>>(ls.ToJson());
            if (sugers == null) sugers = new List<EntriesEntity>();
            foreach (var item in sugers)
            {
                FormatDate(item);
            }

            sugarDTO.curBlood = sugers.Count > 0 ? sugers[0] : new EntriesEntity { date_str = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
            sugarDTO.curBlood.title = NsInfo.title;
            Random rd = new Random();

            if (NsInfo.sayings != null && NsInfo.sayings.Count > 0)
            {
                sugarDTO.curBlood.saying = NsInfo.sayings[rd.Next(0, NsInfo.sayings.Count)];
            }
            else
            {
                sugarDTO.curBlood.saying = "每一次在控制血糖上的成功都是向自己付出的最好回报。";
            }
            {
                //今天
                var flagDate = DateTime.Now.Date;
                sugarDTO.day0 = HandleSugarList(sugers, flagDate);

                if (sugarDTO.day0.Count > 0)
                {
                    var upto = sugarDTO.day0.Where(t => t.sgv_str != null && t.sgv_str.Value > 3.9 && t.sgv_str.Value < 10).ToList().Count();
                    var total = sugarDTO.day0.Where(t => t.sgv_str != null).Count();
                    var percent = Math.Round(1.0 * 100 * upto / total, 0);
                    sugarDTO.curBlood.percent = percent;
                }
                else
                {
                    sugarDTO.curBlood.percent = 0;
                }
            }
            {
                //昨天
                var flagDate = DateTime.Now.Date.AddDays(-1);
                sugarDTO.day1 = HandleSugarList(sugers, flagDate);
            }
            {
                //前天
                var flagDate = DateTime.Now.Date.AddDays(-2);
                sugarDTO.day2 = HandleSugarList(sugers, flagDate);
            }
            return MessageModel<SugarDTO>.Success("", sugarDTO);

        }

        private List<EntriesEntity> HandleSugarList(List<EntriesEntity> sugers, DateTime flagDate)
        {
            List<EntriesEntity> flagList = sugers.Where(t => t.date_now.Date == flagDate).ToList();
            //flagList.Reverse();

            var ls = new List<EntriesEntity>();
            ls.Add(new EntriesEntity { date = (decimal)(flagDate.AddHours(0).AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds, isMask = true });
            ls.Add(new EntriesEntity { date = (decimal)(flagDate.AddHours(3).AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds, isMask = true });
            ls.Add(new EntriesEntity { date = (decimal)(flagDate.AddHours(6).AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds, isMask = true });
            ls.Add(new EntriesEntity { date = (decimal)(flagDate.AddHours(9).AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds, isMask = true });
            ls.Add(new EntriesEntity { date = (decimal)(flagDate.AddHours(12).AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds, isMask = true });
            ls.Add(new EntriesEntity { date = (decimal)(flagDate.AddHours(15).AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds, isMask = true });
            ls.Add(new EntriesEntity { date = (decimal)(flagDate.AddHours(18).AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds, isMask = true });
            ls.Add(new EntriesEntity { date = (decimal)(flagDate.AddHours(21).AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds, isMask = true });
            ls.Add(new EntriesEntity { date = (decimal)(flagDate.AddHours(23).AddMinutes(59).AddSeconds(59).AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds, isMask = true });
            foreach (var item in ls)
            {
                FormatDate(item);
            }
            flagList.AddRange(ls);
            return flagList.AsEnumerable().OrderBy(s => s.date_now).ToList();
        }


        private void FormatDate(EntriesEntity dd)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds((long)dd.date);
            DateTime dateTime = dateTimeOffset.UtcDateTime.AddHours(8);
            dd.date_now = dateTime;
            dd.date_str = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            dd.date_time = dateTime.ToString("HH:mm:ss");

            dd.date_step = (int)(DateTime.Now - dateTime).TotalMinutes;

            if (dd.sgv != null)
                dd.sgv_str = Math.Round(1.0 * dd.sgv.Value / 18, 1);


            dd.direction_str = GetFlag(dd.direction);
        }
        private string GetFlag(string flag)
        {
            switch (flag)
            {
                case "NONE":
                    return "⇼";
                case "TripleUp":
                    return "⤊";
                case "DoubleUp":
                    return "⇈";
                case "SingleUp":
                    return "↑";
                case "FortyFiveUp":
                    return "↗";
                case "Flat":
                    return "→";
                case "FortyFiveDown":
                    return "↘";
                case "SingleDown":
                    return "↓";
                case "DoubleDown":
                    return "⇊";
                case "TripleDown":
                    return "⤋";
                case "NOT COMPUTABLE":
                    return "-";
                case "RATE OUT OF RANGE":
                    return "⇕";
                default:
                    return "未知";
            }
        }


    }

    public class SugarDTO
    {
        public List<EntriesEntity> day0 { get; set; }

        public List<EntriesEntity> day1 { get; set; }

        public List<EntriesEntity> day2 { get; set; }

        public EntriesEntity curBlood { get; set; }

    }

    public class WeChatModel
    {
        public string openid { get; set; }
        public string session_key { get; set; }
        public string unionid { get; set; }
        public int errcode { get; set; }
        public string errmsg { get; set; }
    }

    public class EntriesEntity
    {
        public decimal date { get; set; }

        public DateTime date_now { get; set; }
        public string date_str { get; set; }
        public string date_time { get; set; }
        public int? date_step { get; set; }
        public double? sgv { get; set; }
        public double? sgv_str { get; set; }
        public string direction { get; set; }
        public string direction_str { get; set; }
        public bool isMask { get; set; }
        public string title { get; set; }
        public string saying { get; set; }
        public double percent { get; set; }
    }
    public class AccessTokenDto
    {
        public string access_token { get; set; }
    }

}
