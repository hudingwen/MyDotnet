using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MyDotnet.Common.Cache;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Dto.WeChat;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Domain.Entity.WeChat;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using MyDotnet.Services.Ns;
using MyDotnet.Services.System;
using MyDotnet.Services.WeChat;
using SqlSugar;
using System.Diagnostics;
using System.Globalization;
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
        public DicService _dictService;
        public NightscoutServices _nightscoutServices;

        public WeChatConfigServices _weChatConfigServices;

        public BaseServices<NightscoutLog> _nightscoutLogServices;

        public BaseServices<NightscoutServer> _nightscoutServerServices;

        public BaseServices<NightscoutBanner> _nightscoutBannerServices;
        public BaseServices<NightscoutCustomer> _nightscoutCustomerServices;

        public BaseServices<WeChatSub> _wechatsubServices;

        public UnitOfWorkManage _unitOfWorkManage;
        public ICaching _caching;
        public CodeService _codeService;

        public NightscoutController(NightscoutServices nightscoutServices
            , WeChatConfigServices weChatConfigServices
            , BaseServices<NightscoutLog> nightscoutLogServices
            , BaseServices<NightscoutServer> nightscoutServerServices
            , BaseServices<WeChatSub> wechatsubServices
            , BaseServices<NightscoutBanner> nightscoutBannerServices
            , BaseServices<NightscoutCustomer> nightscoutCustomerServices
            , DicService dictService
            , UnitOfWorkManage unitOfWorkManage
            , ICaching caching
            , CodeService codeService
            )
        {
            _dictService = dictService;
            _nightscoutServices = nightscoutServices;
            _weChatConfigServices = weChatConfigServices;
            _nightscoutLogServices = nightscoutLogServices;
            _nightscoutServerServices = nightscoutServerServices;
            _wechatsubServices = wechatsubServices;
            _nightscoutBannerServices = nightscoutBannerServices;
            _nightscoutCustomerServices = nightscoutCustomerServices;
            _unitOfWorkManage = unitOfWorkManage;
            _caching = caching;
            _codeService = codeService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<MessageModel<string>> StartNS(string host,string key, string code, string pass)
        {
            // 获取当前请求的主机名（包含端口）
            if (string.IsNullOrEmpty(host))
                host = HttpContext.Request.Host.Value;

            if (!_codeService.ValidCode(key, code))
                return MessageModel<string>.Fail($"验证码错误");

            var nightscout = await _nightscoutServices.Dal.Db.Queryable<Nightscout>().Where(t => t.url == host).FirstAsync();
            if (nightscout == null)
            {
                return MessageModel<string>.Fail($"未找到用户:{host}");
            }
            if (!nightscout.passwd.Equals(pass))
            {
                return MessageModel<string>.Fail($"认证失败");
            }
            if (!nightscout.isStop)
            {
                return MessageModel<string>.Fail($"实例已经启动,无需重复启动!");
            }
            if (DateTime.Now > nightscout.endTime)
            {
                return MessageModel<string>.Fail($"NS已过期,请联系续费!");
            }

            var nsServer = await _nightscoutServerServices.Dal.QueryById(nightscout.serverId);
            await _nightscoutServices.Refresh(nightscout, nsServer);
            return MessageModel<string>.Success("启动成功");
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<MessageModel<NsCustomerInfoDto>> GetNsCustomerInfo(string host)
        {
            // 获取当前请求的主机名（包含端口）
            if(string.IsNullOrEmpty(host))
                host = HttpContext.Request.Host.Value;
            NsCustomerInfoDto infoDto = new NsCustomerInfoDto();
            var nightscout =  await _nightscoutServices.Dal.Db.Queryable<Nightscout>().Where(t=>t.url == host).Select(t => new {t.customerId ,t.endTime,t.url,t.isStop}).FirstAsync();
            if(nightscout == null)
            {
                infoDto.showHtml = $"未找到用户:{host}";
                return MessageModel<NsCustomerInfoDto>.Success("获取失败", infoDto);
            }
            else
            {
                var customer = await _nightscoutCustomerServices.Dal.QueryById(nightscout.customerId);
                if(customer == null)
                {
                    infoDto.showHtml = $"未找到客户:{nightscout.customerId}";
                    return MessageModel<NsCustomerInfoDto>.Success("获取失败", infoDto);
                }
                else
                {
                    if(DateTime.Now > nightscout.endTime)
                    {
                        infoDto.isExpire = true;
                    }
                    else
                    {
                        infoDto.isCanShowInput = true;
                    }
                    infoDto.showHtml = customer.introduce;
                    infoDto.endTime = nightscout.endTime;
                    infoDto.host = nightscout.url;
                    infoDto.isStop = nightscout.isStop;
                    return MessageModel<NsCustomerInfoDto>.Success("获取成功", infoDto);
                }
            } 
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<MessageModel<NsPreRemindDto>> GetNsExpireInfo(long id)
        {
            NsPreRemindDto data = new NsPreRemindDto();
            data.uid = id;

            var nightscout = await _nightscoutServices.Dal.Db.Queryable<Nightscout>().Where(t => t.Id == id).Select(t => new { t.name,t.startTime,t.endTime ,t.url}).FirstAsync();
            if (nightscout == null)
            {
                data.isCanShowExpire = false;
                data.showText = "未找到用户";
                return MessageModel<NsPreRemindDto>.Fail("未找到用户", data);
            }
            var preDay = await _dictService.GetDicDataOne(NsInfo.KEY, NsInfo.preDays);
            var front = await _dictService.GetDicDataOne(NsInfo.KEY, NsInfo.frontPage);


            data.name = nightscout.name;
            data.startTime = nightscout.startTime;
            data.endTime = nightscout.endTime;
            data.endTimeStr = data.endTime.ToString("yyyy-MM-dd");
            var pre = nightscout.endTime.AddDays(-preDay.content.ObjToInt());
            data.showTitle = $"(到期时间:{data.endTimeStr})";
            if (DateTime.Now >= nightscout.endTime)
            {
                data.isCanShowExpire = true;
                data.showText = "服务已经到期,请尽快续费,以免服务突然中断!";
            }
            else if (DateTime.Now >= pre)
            {
                data.isCanShowExpire = true;
                data.showText = "服务即将到期,请尽快续费,以免服务突然中断!";
            }
            else
            {
                data.isCanShowExpire = false;
                data.showText = "服务正常";
            }
            var txt = $"<a href=\"{front.content}/?host={nightscout.url}\" style=\"color: #00ff00;\" target=\"_blank\">点击续费</a>";
            data.showText = $"{txt}{data.showText}";
            return MessageModel<NsPreRemindDto>.Success("获取成功",data);
        }
        
        [HttpGet]
        public async Task<MessageModel<string>> ChangeCDN(string cdnCode)
        {
            await _nightscoutServices.ChangeCDN(cdnCode);
            return MessageModel<string>.Success("切换解析成功");
        }

        [HttpGet]
        public async Task<MessageModel<PageModel<Nightscout>>> Get(int page = 1, string key = "",long serverId=0,long? customerId = null, bool? isStop = null, string cdn ="", int size = 10,bool isShowExpire = false,bool isShowSoonExpire=false)
        {


            Expression<Func<Nightscout, bool>> whereExpression = a => true;
            key = key.ObjToString().Trim();

            if (!string.IsNullOrEmpty(key))
            {
                whereExpression = whereExpression.And(t => t.name.Contains(key) || t.url.Contains(key) || t.status.Contains(key) || t.resource.Contains(key) || t.position.Contains(key));
            }
            if (!string.IsNullOrEmpty(cdn))
            {
                whereExpression = whereExpression.And(t => t.cdn == cdn);
            }
            if (serverId > 0)
            {
                whereExpression = whereExpression.And(t => t.serverId == serverId);
            }
            if (customerId != null)
            {
                whereExpression = whereExpression.And(t => t.customerId == customerId);
            }
            if(isStop != null)
            {
                whereExpression = whereExpression.And(t => t.isStop == isStop);
            }
            //显示过期
            if (isShowExpire)
            {
                whereExpression = whereExpression.And(t => DateTime.Now.Date >= t.endTime);
            }
            //显示即将过期
            if (isShowSoonExpire)
            {
                var preDayInfo = await _dictService.GetDicDataOne(NsInfo.KEY, NsInfo.preDays);
                var preDay = preDayInfo.content.ObjToInt(); 

                whereExpression = whereExpression.And(t =>  t.endTime > DateTime.Now.Date && t.endTime < DateTime.Now.Date.AddDays(preDay));
            }
            var data = await _nightscoutServices.Dal.QueryPage(whereExpression, page, size);

            //ip查询
            var setvers =  await _nightscoutServerServices.Dal.Query();
            foreach (var item in data.data)
            {
                var server = setvers.Find(t => t.Id == item.serverId);
                item.instanceIP = server.serverIp;
            }

            var nsInfo = await _dictService.GetDicData(NsInfo.KEY);
            var pushWechatID = nsInfo.Find(t => t.code.Equals(NsInfo.pushWechatID)).content;
            var pushCompanyCode = nsInfo.Find(t => t.code.Equals(NsInfo.pushCompanyCode)).content;

            var bindData = await _wechatsubServices.Dal.Query(t => t.SubFromPublicAccount == pushWechatID && t.IsUnBind == false && t.CompanyID == pushCompanyCode && data.data.Select(i => i.Id.ToString()).ToList().Contains(t.SubJobID));
            //绑定微信检测
            foreach (var item in data.data)
            {
                var find = bindData.Find(t => t.SubJobID.Equals(item.Id.ToString()));
                if (find != null)
                {
                    item.isBindWeChat = true;
                }
            }

            var miniPro = await _dictService.GetDicData(NSminiProgram.KEY);
            var miniAppid = miniPro.Find(t => t.code.Equals(NSminiProgram.appid)).content;

             

            var bindMini = await _wechatsubServices.Dal.Query(t => t.SubFromPublicAccount == miniAppid && t.IsUnBind == false && t.CompanyID == pushCompanyCode && data.data.Select(i => i.Id.ToString()).ToList().Contains(t.SubJobID));
            //绑定小程序检测
            foreach (var item in data.data)
            {
                var find = bindMini.Find(t => t.SubJobID.Equals(item.Id.ToString()));
                if (find != null)
                {
                    item.isBindMini = true; 
                    
                    item.miniUrl = $"{miniPro.Find(t=>t.code.Equals(NSminiProgram.url)).content }?openid={find.SubUserOpenID}";
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
            request.lastUpdateTime = request.startTime;
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

            //whereExpression = a => true;
            //whereExpression = whereExpression.And(t => t.Id != request.Id);
            //whereExpression = whereExpression.And(t => t.exposedPort == 0 && t.instanceIP == request.instanceIP && t.serverId == request.serverId);
            //data = await _nightscoutServices.Dal.Query(whereExpression);
            //if (data != null && data.Count > 0)
            //    return MessageModel<string>.Fail($"当前操作的[{request.name}]与[{data[0].name}]的IP配置有冲突,请检查");

            //whereExpression = a => true;
            //whereExpression = whereExpression.And(t => t.Id != request.Id);
            //whereExpression = whereExpression.And(t => t.exposedPort > 0 && t.exposedPort == request.exposedPort && t.serverId == request.serverId);
            //data = await _nightscoutServices.Dal.Query(whereExpression);
            //if (data != null && data.Count > 0)
            //    return MessageModel<string>.Fail($"当前操作的[{request.name}]与[{data[0].name}]的端口配置有冲突,请检查");

            whereExpression = a => true;
            whereExpression = whereExpression.And(t => t.Id != request.Id);
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
                //获取当前最大服务序列
                var curServiceNameSerial = await _dictService.GetDic(DicTypeList.NsServiceNameCurSerial,false);
                var nsserver = await _nightscoutServerServices.Dal.QueryById(request.serverId);
                if (request.serviceSerial == 0)
                {
                    //新增实例
                    var curSerial = curServiceNameSerial.content.ObjToInt();
                    if (curSerial <= 0)
                    {
                        return MessageModel<string>.Fail("实例序列初始序列未设置");
                    }
                    curSerial += 1;
                    curServiceNameSerial.content = curSerial.ObjToString();
                    //设置服务名称
                    request.serviceName = $"nightscout-template{curSerial}";
                    request.serviceSerial = curSerial;
                    //通过暴露端口访问
                    if (nsserver.curExposedPort <= 0)
                    {
                        return MessageModel<string>.Fail("服务器端口初始端口未设置");
                    }
                    nsserver.curExposedPort += 1;
                    request.exposedPort = nsserver.curExposedPort;
                    request.instanceIP = nsserver.serverIp;

                    //不填写自动生成
                    var padName = curSerial.ObjToString().PadLeft(3, '0');
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
                        var nsInfo = await _dictService.GetDicData(NsInfo.KEY);
                        request.url = string.Format(nsInfo.Find(t=>t.code.Equals(NsInfo.templateUrl)).content , GenerateNumber(3) + padName);
                    }
                    if (string.IsNullOrEmpty(request.cdn))
                    {
                        var defaultCDN = await _dictService.GetDic(DicTypeList.defaultCDN);
                        request.cdn = defaultCDN.content;
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
                    await _dictService.PutDicType(curServiceNameSerial);
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
                        //添加解析
                        await _nightscoutServices.ResolveDomain(request);
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
                data.response = request.Id.ObjToString();
            }
            bool isDiff = false;

            //是否切换域名
            if (!request.url.Equals(old.url))
            {
                await _nightscoutServices.UnResolveDomain(old);
                await _nightscoutServices.ResolveDomain(request);
            }
            else
            {
                //是否切换解析
                if (!request.cdn.Equals(old.cdn))
                {
                    await _nightscoutServices.UnResolveDomain(old);
                    await _nightscoutServices.ResolveDomain(request);
                }
            }

            
            if (!request.serverId.Equals(old.serverId))
            {
                //不是同一个服务器需要停掉先前服务器

                var oldNsserver = await _nightscoutServerServices.Dal.QueryById(old.serverId);
                await _nightscoutServices.StopDocker(old, oldNsserver);

                var nsserver = await _nightscoutServerServices.Dal.QueryById(request.serverId);
                //通过暴露端口访问
                if (nsserver.curExposedPort <= 0)
                {
                    return MessageModel<string>.Fail("服务器端口初始端口未设置");
                }
                nsserver.curExposedPort += 1;
                request.exposedPort = nsserver.curExposedPort;
                request.instanceIP = nsserver.serverIp;

                var check2 = await CheckData(request);
                if (!check2.success)
                {
                    check.msg = $"数据更新成功,但是服务器迁移失败,请手动处理!{check.msg}";
                    return check;
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
                //判断是否迁移数据
                var isSwitchOk = await _nightscoutServices.SwitchDatabase(request, oldNsserver, nsserver);
                if (isSwitchOk)
                {
                    //迁移成功后删除原数据库
                    await _nightscoutServices.DeleteData(request, oldNsserver);
                }
                //启动新实例
                if(!request.isStop)
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
                data.response = model.Id.ObjToString();
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

            data.isStop = false;
            await _nightscoutServices.Dal.Update(data, t => new { t.isStop });
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
            await _nightscoutServices.Refresh(data, nsserver);
            if(DateTime.Now > data.endTime)
            {
                return MessageModel<string>.Success("刷新成功,用户使用期限已到,请注意!");
            }
            else
            {
                return MessageModel<string>.Success("刷新成功");
            }
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
            var nsInfo = await _dictService.GetDicData(NsInfo.KEY);
            var pushCompanyCode = nsInfo.Find(t => t.code.Equals(NsInfo.pushCompanyCode)).content;
            var pushWechatID = nsInfo.Find(t => t.code.Equals(NsInfo.pushWechatID)).content;
            return await _weChatConfigServices.GetQRBind(new WeChatUserInfo { userID = id.ToString(), companyCode = pushCompanyCode, id = pushWechatID, userNick = data.name });
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
            var nsInfo = await _dictService.GetDicData(NsInfo.KEY);
            var pushCompanyCode = nsInfo.Find(t => t.code.Equals(NsInfo.pushCompanyCode)).content;
            var pushWechatID = nsInfo.Find(t => t.code.Equals(NsInfo.pushWechatID)).content;
            return await _weChatConfigServices.UnBind(new WeChatUserInfo { userID = id.ToString(), companyCode = pushCompanyCode, id = pushWechatID });
        }

        /// <summary>
        /// 获取实例运行日志
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<NightscoutLog>>> GetLog(long id, int page = 1, int size = 10)
        {

            var data = await _nightscoutServices.Dal.QueryById(id);
            if (data == null || data.IsDeleted) return MessageModel<PageModel<NightscoutLog>>.Fail("实例不存在");
            Expression<Func<NightscoutLog, bool>> whereExpression = a => true;
            whereExpression = whereExpression.And(t => t.pid == id);
            return new MessageModel<PageModel<NightscoutLog>>()
            {
                msg = "获取成功",
                success = true,
                response = await _nightscoutLogServices.Dal.QueryPage(whereExpression, page, size, "Id desc")
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
                var nsInfo = await _dictService.GetDicData(NsInfo.KEY);
                var pushTemplateID_Keep = nsInfo.Find(t => t.code.Equals(NsInfo.pushTemplateID_Keep)).content;

                pushTemplateID = pushTemplateID_Keep;
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
                var nsInfo = await _dictService.GetDicData(NsInfo.KEY);
                var pushTemplateID_Exception = nsInfo.Find(t => t.code.Equals(NsInfo.pushTemplateID_Exception)).content;
                pushTemplateID = pushTemplateID_Exception;
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

            var miniPro = await _dictService.GetDicData(NSminiProgram.KEY);
            pushData.cardMsg.miniprogram = new WeChatCardMsgMiniprogram { appid = miniPro.Find(t=>t.code.Equals(NSminiProgram.appid)).content, pagepath = miniPro.Find(t => t.code.Equals(NSminiProgram.path)).content };


            var dicNsInfo = await _dictService.GetDicData(NsInfo.KEY);
            var pushCompanyCode = dicNsInfo.Find(t => t.code.Equals(NsInfo.pushCompanyCode)).content;
            var pushWechatID = dicNsInfo.Find(t => t.code.Equals(NsInfo.pushWechatID)).content;

            pushData.info = new WeChatUserInfo();
            pushData.info.id = pushWechatID;
            pushData.info.companyCode = pushCompanyCode;
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


            var customer = await _nightscoutServices.Dal.Db.Queryable<Nightscout>()
             .GroupBy(it => it.customerId)
             .Select(it => new
             {
                 count = SqlFunc.AggregateCount(it.Id),
                 customerId = it.customerId
             })
             .ToListAsync();

            return MessageModel<object>.Success("获取成功", new { status, resource, customer });
        }
        [HttpGet]
        public async Task<MessageModel<List<NSPlugin>>> GetPlugins()
        {
            var nsPlugins = await _dictService.GetDicData(NSplugins.KEY);
            List<NSPlugin> plugins = nsPlugins.Select(t => new NSPlugin { key = t.code, name = t.name }).ToList();
            return MessageModel<List<NSPlugin>>.Success("成功", plugins);
        }



        [HttpGet]
        public async Task<MessageModel<PageModel<NightscoutServer>>> getNsServer(int page = 1, string key = "", int size = 10)
        {
            Expression<Func<NightscoutServer, bool>> whereExpression = a => true;

            if (!(string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key)))
            {
                key = key.Trim();
                whereExpression = whereExpression.And(t => t.serverName.Contains(key));
            }
            var data = await _nightscoutServerServices.Dal.QueryPage(whereExpression, page, size,t=>t.serverName, OrderByType.Asc);
            return MessageModel<PageModel<NightscoutServer>>.Success("获取成功", data);
        }
        [HttpDelete]
        public async Task<MessageModel<string>> delNsServer(string id)
        {
            await _nightscoutServerServices.Dal.DeleteById(id);
            await _caching.RemoveAsync(CacheKeyList.allNsServer);
            return MessageModel<string>.Success("删除成功");
        }

        [HttpPost]
        public async Task<MessageModel<string>> delBatchNsServer([FromBody] string[] ids)
        {
            await _nightscoutServerServices.Dal.DeleteByIds(ids);
            await _caching.RemoveAsync(CacheKeyList.allNsServer);
            return MessageModel<string>.Success("删除成功");
        }
        [HttpPut]
        public async Task<MessageModel<string>> updateNsServer([FromBody] NightscoutServer request)
        {
            var data = await _nightscoutServerServices.Dal.QueryById(request.Id);
            if (data == null) MessageModel<string>.Fail("要更新的数据不存在");
            var id = await _nightscoutServerServices.Dal.Update(request);
            await _caching.RemoveAsync(CacheKeyList.allNsServer);
            return MessageModel<string>.Success("更新成功", id.ObjToString());
        }
        [HttpPost]
        public async Task<MessageModel<string>> addNsServer([FromBody] NightscoutServer request)
        {
            var id = await _nightscoutServerServices.Dal.Add(request);
            await _caching.RemoveAsync(CacheKeyList.allNsServer);
            return MessageModel<string>.Success("添加成功", id.ObjToString());
        }




        [HttpGet]
        public async Task<MessageModel<PageModel<NightscoutBanner>>> getNsBanner(int page = 1, string key = "", int size = 10)
        {
            Expression<Func<NightscoutBanner, bool>> whereExpression = a => true;

            if (!(string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key)))
            {
                key = key.Trim();
                whereExpression = whereExpression.And(t => t.title.Contains(key) || t.content.Contains(key));
            }
            var data = await _nightscoutBannerServices.Dal.QueryPage(whereExpression, page, size);
            return MessageModel<PageModel<NightscoutBanner>>.Success("获取成功", data);
        }
        [HttpDelete]
        public async Task<MessageModel<string>> delNsBanner(string id)
        {
            await _nightscoutBannerServices.Dal.DeleteById(id);
            return MessageModel<string>.Success("删除成功");
        }
        [HttpPost]
        public async Task<MessageModel<string>> delBatchNsBanner([FromBody] string[] ids)
        {
            await _nightscoutBannerServices.Dal.DeleteByIds(ids);
            return MessageModel<string>.Success("删除成功");
        }
        [HttpPut]
        public async Task<MessageModel<string>> updateNsBanner([FromBody] NightscoutBanner request)
        {
            var data = await _nightscoutBannerServices.Dal.QueryById(request.Id);
            if (data == null) MessageModel<string>.Fail("要更新的数据不存在");
            var id = await _nightscoutBannerServices.Dal.Update(request);
            return MessageModel<string>.Success("更新成功", id.ObjToString());
        }
        [HttpPost]
        public async Task<MessageModel<string>> addNsBanner([FromBody] NightscoutBanner request)
        {
            var id = await _nightscoutBannerServices.Dal.Add(request);
            return MessageModel<string>.Success("添加成功", id.ObjToString());
        }
        [HttpPost]
        public async Task<MessageModel<string>> enableAllBanner()
        {
            await _nightscoutBannerServices.Dal.Db.Updateable<NightscoutBanner>().SetColumns(t=>t.Enabled, true).Where(t => t.Id > 0).ExecuteCommandAsync();
            return MessageModel<string>.Success("更新成功");
        }
        [HttpPost]
        public async Task<MessageModel<string>> disableAllBanner()
        {
            await _nightscoutBannerServices.Dal.Db.Updateable<NightscoutBanner>().SetColumns(t => t.Enabled, false).Where(t => t.Id > 0).ExecuteCommandAsync();
            return MessageModel<string>.Success("更新成功");
        }



        [HttpGet]
        public async Task<MessageModel<PageModel<NightscoutCustomer>>> getNsCustomer(int page = 1, string key = "", int size = 10)
        {
            Expression<Func<NightscoutCustomer, bool>> whereExpression = a => true;

            if (!(string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key)))
            {
                key = key.Trim();
                whereExpression = whereExpression.And(t => t.name.Contains(key) || t.tel.Contains(key));
            }
            var data = await _nightscoutCustomerServices.Dal.QueryPage(whereExpression, page, size);
            return MessageModel<PageModel<NightscoutCustomer>>.Success("获取成功", data);
        }
        [HttpDelete]
        public async Task<MessageModel<string>> delNsCustomer(string id)
        {
            await _nightscoutCustomerServices.Dal.DeleteById(id);
            return MessageModel<string>.Success("删除成功");
        }
        [HttpPost]
        public async Task<MessageModel<string>> delBatchNsCustomer([FromBody] string[] ids)
        {
            await _nightscoutCustomerServices.Dal.DeleteByIds(ids);
            return MessageModel<string>.Success("删除成功");
        }
        [HttpPut]
        public async Task<MessageModel<string>> updateNsCustomer([FromBody] NightscoutCustomer request)
        {
            var data = await _nightscoutCustomerServices.Dal.QueryById(request.Id);
            if (data == null) MessageModel<string>.Fail("要更新的数据不存在");
            var id = await _nightscoutCustomerServices.Dal.Update(request);
            return MessageModel<string>.Success("更新成功", id.ObjToString());
        }
        [HttpPost]
        public async Task<MessageModel<string>> addNsCustomer([FromBody] NightscoutCustomer request)
        {
            var id = await _nightscoutCustomerServices.Dal.Add(request);
            return MessageModel<string>.Success("添加成功", id.ObjToString());
        }





        [HttpGet]
        public async Task<MessageModel<List<NightscoutServer>>> getAllNsServer()
        {
            var data = await _caching.GetAsync<List<NightscoutServer>>(CacheKeyList.allNsServer);
            if(data == null)
            {
                //缓存穿透
                data = await _nightscoutServerServices.Dal.Db.Queryable<NightscoutServer>().OrderBy(t=>t.serverName).Select(t => new NightscoutServer { Id = t.Id, serverName = t.serverName, holdCount = t.holdCount }).ToListAsync();
                await _caching.SetAsync(CacheKeyList.allNsServer, data);
            }


            var ids = data.Select(tt => tt.Id).ToList();
            //
            var ls = await _nightscoutServerServices.Dal.Db.Queryable<Nightscout>()
            .GroupBy(t => new { t.serverId ,t.isStop})
                .Select(t => new { t.serverId,t.isStop,count = SqlFunc.AggregateCount(t.serverId) }).ToListAsync();

            foreach (var item in data)
            {
                var rows = ls.FindAll(t => t.serverId == item.Id);
                if(rows.Count() > 0)
                {
                    foreach (var tj in rows)
                    {
                        item.count += tj.count;
                        if (tj.isStop)
                        {
                            item.countStop = tj.count;
                        }
                        else
                        {
                            item.countStart = tj.count;
                        }

                    }
                }
                else
                {
                    item.count = 0;
                    item.countStart = 0;
                    item.countStop = 0;
                }
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

            var miniPro = await _dictService.GetDicData(NSminiProgram.KEY);

            var weChatToken = new { appid = miniPro.Find(t=>t.code.Equals(NSminiProgram.appid)).content , secret = miniPro.Find(t => t.code.Equals(NSminiProgram.secret)).content, grant_type = miniPro.Find(t => t.code.Equals(NSminiProgram.miniGrantType)).content };

            var nsInfo = await _nightscoutServices.Dal.QueryById(nsid);



            var dicNsInfo = await _dictService.GetDicData(NsInfo.KEY);
            var pushCompanyCode = dicNsInfo.Find(t => t.code.Equals(NsInfo.pushCompanyCode)).content; 

            WeChatQR weChatQR = new WeChatQR
            {
                QRbindCompanyID = pushCompanyCode,
                QRbindJobID = nsInfo.Id.ObjToString(),
                QRbindJobNick = nsInfo.name,
                QRcrateTime = DateTime.Now,
                QRpublicAccount = miniPro.Find(t => t.code.Equals(NSminiProgram.appid)).content,
                QRticket = StringHelper.GetGUID()
            };

            await _weChatConfigServices.Dal.Db.Insertable<WeChatQR>(weChatQR).ExecuteCommandAsync();


            //HttpContent httpContent = new StringContent(JsonHelper.ObjToJson(weChatToken));
            //string result = await HttpHelper.PostAsync(url, httpContent);
            //AccessTokenDto accessTokenDto = JsonHelper.JsonToObj<AccessTokenDto>(result);

            ////正式版为 "release"，体验版为 "trial"，开发版为 "develop"
            //var ticket = weChatQR.QRticket;
            //var jsonBind = JsonHelper.ObjToJson(new { path = $"pages/index/index?ticket={ticket}", env_version = NsInfo.miniEnv, width = 128 });
            //HttpContent httpContentBind = new StringContent(jsonBind);
            //var urlBind = $"https://api.weixin.qq.com/wxa/getwxacode?access_token={accessTokenDto.access_token}";

            //var bindstream = await HttpHelper.PostAsync(urlBind, httpContentBind);
            //return File(bindstream, "image/jpeg");
            //return MessageModel<string>.Success("成功", bindstream);




            string result;
            using (HttpContent httpContent = new StringContent(JsonHelper.ObjToJson(weChatToken)))
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = new TimeSpan(0, 0, 60);
                    result = await httpClient.PostAsync(url, httpContent).Result.Content.ReadAsStringAsync();
                    AccessTokenDto accessTokenDto = JsonHelper.JsonToObj<AccessTokenDto>(result);

                    //正式版为 "release"，体验版为 "trial"，开发版为 "develop"
                    var ticket = weChatQR.QRticket;
                    var jsonBind = JsonHelper.ObjToJson(new { path = $"pages/index/index?ticket={ticket}", env_version = miniPro.Find(t => t.code.Equals(NSminiProgram.env)).content, width = 128 });
                    using (HttpContent httpContentBind = new StringContent(jsonBind))
                    {
                        var urlBind = $"https://api.weixin.qq.com/wxa/getwxacode?access_token={accessTokenDto.access_token}";
                        var bindstream = await httpClient.PostAsync(urlBind, httpContentBind).Result.Content.ReadAsByteArrayAsync();
                        //return File(bindstream, "image/jpeg");
                        return MessageModel<string>.Success("成功", Convert.ToBase64String(bindstream));
                    }
                }
            }


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
            var dicNsInfo = await _dictService.GetDicData(NsInfo.KEY);
            var pushCompanyCode = dicNsInfo.Find(t => t.code.Equals(NsInfo.pushCompanyCode)).content;

            var miniPro = await _dictService.GetDicData(NSminiProgram.KEY);
            var miniAppid = miniPro.Find(t => t.code.Equals(NSminiProgram.appid)).content;

            var nsInfo = await _weChatConfigServices.Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == miniAppid && t.CompanyID == pushCompanyCode && t.SubUserOpenID == openid).FirstAsync();
            if(nsInfo != null && nsInfo.IsUnBind == false) return MessageModel<string>.Success("您已绑定,无需重复绑定!");

            var ticketInfo = await _weChatConfigServices.Dal.Db.Queryable<WeChatQR>().Where(t => t.QRpublicAccount == miniAppid && t.QRticket == ticket).FirstAsync();
            if (ticketInfo == null)
                return MessageModel<string>.Fail($"无效的绑定信息");
            if (ticketInfo.QRisUsed)
                return MessageModel<string>.Fail($"绑定码已使用,请勿重复使用!");



            
            var bindInfo = await _weChatConfigServices.Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == miniAppid && t.CompanyID == pushCompanyCode && t.SubJobID == ticketInfo.QRbindJobID).FirstAsync();

           
            try
            {
                _unitOfWorkManage.BeginTran();

                ticketInfo.QRisUsed = true;
                ticketInfo.QRuseTime = DateTime.Now;
                ticketInfo.QRuseOpenid = openid;
                await _weChatConfigServices.Dal.Db.Updateable<WeChatQR>(ticketInfo).UpdateColumns(t => new { t.QRisUsed, t.QRuseOpenid, t.QRuseTime }).ExecuteCommandAsync();


                if (bindInfo == null)
                {
                    //新增绑定信息
                    bindInfo = new WeChatSub
                    {
                        SubFromPublicAccount = miniAppid,
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
                    //更新绑定信息
                    bindInfo.LastSubUserOpenID = bindInfo.SubUserOpenID;
                    bindInfo.SubUserOpenID = openid;
                    bindInfo.SubUserRefTime = DateTime.Now;
                    bindInfo.IsUnBind = false;
                    await _weChatConfigServices.Dal.Db.Updateable<WeChatSub>(bindInfo).UpdateColumns(t => new { t.LastSubUserOpenID, t.SubUserOpenID, t.SubUserRefTime, t.IsUnBind }).ExecuteCommandAsync();
                }
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
        /// 获取小程序修复二维码
        /// </summary>
        /// <param name="nsid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> GetFixQR(long nsid)
        {
            string url = $"https://api.weixin.qq.com/cgi-bin/stable_token";

            var miniPro = await _dictService.GetDicData(NSminiProgram.KEY);

            var weChatToken = new { appid = miniPro.Find(t => t.code.Equals(NSminiProgram.appid)).content, secret = miniPro.Find(t => t.code.Equals(NSminiProgram.secret)).content, grant_type = miniPro.Find(t => t.code.Equals(NSminiProgram.miniGrantType)).content };

            var nsInfo = await _nightscoutServices.Dal.QueryById(nsid);



            var miniAppid = miniPro.Find(t => t.code.Equals(NSminiProgram.appid)).content;

            var pushCompanyCode = (await _dictService.GetDicDataOne(NsInfo.KEY, NsInfo.pushCompanyCode)).content;


            var bindMini = await _wechatsubServices.Dal.Query(t => t.SubFromPublicAccount == miniAppid && t.IsUnBind == false && t.CompanyID == pushCompanyCode && t.SubJobID == nsInfo.Id.ObjToString());



            if (bindMini == null || bindMini.Count == 0) return MessageModel<string>.Fail("未找到绑定用户");

            string result;
            using (HttpContent httpContent = new StringContent(JsonHelper.ObjToJson(weChatToken)))
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = new TimeSpan(0, 0, 60);
                    result = await httpClient.PostAsync(url, httpContent).Result.Content.ReadAsStringAsync();
                    AccessTokenDto accessTokenDto = JsonHelper.JsonToObj<AccessTokenDto>(result);

                    //正式版为 "release"，体验版为 "trial"，开发版为 "develop" 
                    var jsonBind = JsonHelper.ObjToJson(new { path = $"pages/index/index?uid={bindMini[0].SubUserOpenID}", env_version = miniPro.Find(t => t.code.Equals(NSminiProgram.env)).content, width = 128 });
                    using (HttpContent httpContentBind = new StringContent(jsonBind))
                    {
                        var urlBind = $"https://api.weixin.qq.com/wxa/getwxacode?access_token={accessTokenDto.access_token}";
                        var bindstream = await httpClient.PostAsync(urlBind, httpContentBind).Result.Content.ReadAsByteArrayAsync();
                        //return File(bindstream, "image/jpeg");
                        return MessageModel<string>.Success("成功", Convert.ToBase64String(bindstream));
                    }
                }
            }


        }
        /// <summary>
        /// 取消绑定小程序
        /// </summary>
        /// <param name="nsid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> UnBindQR(string nsid)
        {
            var miniPro = await _dictService.GetDicData(NSminiProgram.KEY);
            var miniAppid = miniPro.Find(t => t.code.Equals(NSminiProgram.appid)).content;

            var nsInfo = await _dictService.GetDicData(NsInfo.KEY);
            var pushCompanyCode = nsInfo.Find(t => t.code.Equals(NsInfo.pushCompanyCode)).content;


            var bindInfo = await _weChatConfigServices.Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == miniAppid && t.SubJobID == nsid && t.CompanyID == pushCompanyCode && t.IsUnBind == false).ToListAsync();
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

            var appid = await _dictService.GetDicData(NSminiProgram.KEY, NSminiProgram.appid);

            var secret = await _dictService.GetDicData(NSminiProgram.KEY, NSminiProgram.secret);

            var miniGrantType = await _dictService.GetDicData(NSminiProgram.KEY, NSminiProgram.miniGrantType); 

            var res = await HttpHelper.GetAsync($"https://api.weixin.qq.com/sns/jscode2session?appid={appid.content}&secret={secret.content}&js_code={code}&grant_type={miniGrantType.content}");

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


            var miniPro = await _dictService.GetDicData(NSminiProgram.KEY);
            var miniAppid = miniPro.Find(t => t.code.Equals(NSminiProgram.appid)).content;

            var nsInfo = await _dictService.GetDicData(NsInfo.KEY);
            var pushCompanyCode = nsInfo.Find(t => t.code.Equals(NsInfo.pushCompanyCode)).content;


            var nsid = await _weChatConfigServices.Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == miniAppid && t.CompanyID == pushCompanyCode && t.SubUserOpenID == openid && t.IsUnBind == false).Select(t => t.SubJobID).FirstAsync();

            if (string.IsNullOrEmpty(nsid))
                return MessageModel<SugarDTO>.Fail("无绑定信息,无法查看血糖");
            var longNsid = nsid.ObjToLong();
            var ns = await _weChatConfigServices.Dal.Db.Queryable<Nightscout>().Where(t => t.Id == longNsid).Select(t => new { t.serviceName,t.serverId,t.probeStartTime }).FirstAsync();
            if (ns ==null)
                return MessageModel<SugarDTO>.Fail("无血糖信息可供查看,请检查是否绑定NS");

            var nsServer = await _nightscoutServerServices.Dal.Db.Queryable<NightscoutServer>().Where(t=>t.Id == ns.serverId).Select(t=> new { t.mongoServerId }).FirstAsync();
            var curNsserverMongoSsh = await _nightscoutServerServices.Dal.QueryById(nsServer.mongoServerId);

            var grantConnectionMongoString = $"mongodb://{curNsserverMongoSsh.mongoLoginName}:{curNsserverMongoSsh.mongoLoginPassword}@{curNsserverMongoSsh.mongoIp}:{curNsserverMongoSsh.mongoPort}";

            var client = new MongoClient(grantConnectionMongoString);
            var database = client.GetDatabase(ns.serviceName);
            var collection = database.GetCollection<BsonDocument>("entries"); // 替换为你的集合名称

            //var filter = Builders<MongoDB.Bson.BsonDocument>.Filter.Empty; // 获取所有数据.Eq("name", "John");
            var filter = Builders<BsonDocument>.Filter.Gt("sgv", 0);
            var projection = Builders<BsonDocument>.Projection.Include("date").Include("sgv").Include("direction").Include("utcOffset").Exclude("_id");


            var ls = await collection.Find(filter).Sort(Builders<BsonDocument>.Sort.Descending("date")).Limit(900).Project(projection).ToListAsync();


            var sugers = JsonHelper.JsonToObj<List<EntriesEntity>>(ls.ToJson());
            if (sugers == null) sugers = new List<EntriesEntity>();
            foreach (var item in sugers)
            {
                FormatDate(item);
            }

            if (sugers.Count > 0)
            {
                sugarDTO.curBlood = sugers[0];
                if (sugarDTO.curBlood.date_step >= 5 || sugarDTO.curBlood.date_step < 0)
                {
                    DateTime utcTime = DateTime.Now.AddMinutes(1).ToUniversalTime();
                    TimeSpan timeSpan = utcTime - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
                    sugarDTO.curBlood.nextRefreshTime = timeSpan.TotalMilliseconds;
                }
                else
                {
                    DateTime utcTime = DateTime.Now.AddMinutes(5 - sugarDTO.curBlood.date_step.Value).ToUniversalTime();
                    TimeSpan timeSpan = utcTime - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
                    sugarDTO.curBlood.nextRefreshTime = timeSpan.TotalMilliseconds;
                }
            }
            else
            {
                sugarDTO.curBlood = new EntriesEntity { date_str = DateTime.Now, date_step = -1 };

                DateTime utcTime = DateTime.Now.AddMinutes(1).ToUniversalTime();
                TimeSpan timeSpan = utcTime - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
                sugarDTO.curBlood.nextRefreshTime = timeSpan.TotalMilliseconds;
            }


            sugarDTO.curBlood.probeStartTime = ns.probeStartTime;
            if(sugarDTO.curBlood.probeStartTime != null)
            {
                var useProbe = DateTime.Now - sugarDTO.curBlood.probeStartTime.Value;
                sugarDTO.curBlood.probeUseDays = useProbe.Days;
                sugarDTO.curBlood.probeUseHours = useProbe.Hours;
                sugarDTO.curBlood.probeUseMinutes = useProbe.Minutes;
            }


            sugarDTO.curBlood.title = miniPro.Find(t => t.code.Equals(NSminiProgram.title)).content;
            Random rd = new Random();

            var sayings = await _nightscoutBannerServices.Dal.Query(t => t.Enabled==true);
            if (sayings != null && sayings.Count > 0)
            {
                sugarDTO.curBlood.saying = sayings[rd.Next(0, sayings.Count)].content;
            }
            else
            {
                sugarDTO.curBlood.saying = "每一次在控制血糖上的成功都是向自己付出的最好回报。";
            }
            if (sugers.Count > 0)
            {
                var upto = sugers.Where(t => t.sgv_str != null && t.sgv_str.Value > 3.9 && t.sgv_str.Value < 10).ToList().Count();
                var total = sugers.Where(t => t.sgv_str != null).Count();
                var percent = Math.Round(1.0 * 100 * upto / total, 0);
                sugarDTO.curBlood.percent = percent;

                
            }
            else
            {
                sugarDTO.curBlood.percent = 0;
            }
            //添加标识
            sugarDTO.day0 = HandleSugarList(sugers, sugarDTO);
            return MessageModel<SugarDTO>.Success("", sugarDTO);

        }

        /// <summary>
        /// 更新小程序使用探头时间
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="time"></param>
        /// <returns></returns>

        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<SugarDTO>> UpdateMyProbeStartTime(string openid,DateTime? time)
        {
            var miniPro = await _dictService.GetDicData(NSminiProgram.KEY);
            var miniAppid = miniPro.Find(t => t.code.Equals(NSminiProgram.appid)).content;

            var nsInfo = await _dictService.GetDicData(NsInfo.KEY);
            var pushCompanyCode = nsInfo.Find(t => t.code.Equals(NsInfo.pushCompanyCode)).content;


            var nsid = await _weChatConfigServices.Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == miniAppid && t.CompanyID == pushCompanyCode && t.SubUserOpenID == openid && t.IsUnBind == false).Select(t => t.SubJobID).FirstAsync();

            if (string.IsNullOrEmpty(nsid))
                return MessageModel<SugarDTO>.Fail("无绑定信息,无法查看血糖");
            var longNsid = nsid.ObjToLong();
            var ns = await _weChatConfigServices.Dal.Db.Queryable<Nightscout>().Where(t => t.Id == longNsid).Select(t => new { t.Id }).FirstAsync();
            if (ns == null)
                return MessageModel<SugarDTO>.Fail("无血糖信息可供查看,请检查是否绑定NS");


            await _weChatConfigServices.Dal.Db.Updateable<Nightscout>().SetColumns(t => t.probeStartTime, time).Where(t => t.Id == ns.Id).ExecuteCommandAsync();

            return MessageModel<SugarDTO>.Success("成功成功");
        }

            [HttpGet]
        public async Task<MessageModel<CDNInfoDto>> GetCDNList()
        {
            var defaultCDN = await _dictService.GetDic(DicTypeList.defaultCDN);
            var cdnList = await _dictService.GetDicData(CDNList.KEY);

            CDNInfoDto cDNInfoDto = new CDNInfoDto { CDNList = cdnList.Select(t => new NSCDN { key = t.code, name = t.name, type = t.content, value = t.content2 }).ToList(), defaultCDN = defaultCDN.content };
            return MessageModel<CDNInfoDto>.Success("获取成功", cDNInfoDto);
        }
        private List<EntriesEntity> HandleSugarList(List<EntriesEntity> sugers, SugarDTO sugarDTO)
        {
            sugers = sugers.OrderBy(s => s.date_str).ToList();
            //分组日期
            var data = sugers.GroupBy(t => t.date_str.Date).ToList();
            sugarDTO.groupDays = data.Select(t=> t.Key.ToString("yyyy-MM-dd")).ToList();
            int idx = 1;
            foreach (var day in data)
            {
                
                var dayAll = sugers.FindAll(t => t.date_str.Date == day.Key);
                var dayAllPercent = dayAll.FindAll(t => t.sgv_str >= 3.9 && t.sgv_str <= 10);
                if(dayAll.Count == 0)
                {
                    sugarDTO.groupDaysPercent.Add("0%");
                }
                else
                {
                    if (data.Count>3 && idx == 1)
                    {
                        sugarDTO.groupDaysPercent.Add("数据不足");
                    }
                    else
                    {
                        sugarDTO.groupDaysPercent.Add(((int)((1.0 * dayAllPercent.Count / dayAll.Count) * 100)).ToString() + "%");
                    }
                }
                for (int i = 0; i < dayAll.Count; i++)
                {
                    if (i == 0)
                    {
                        var dat = (int)dayAll[i].date_str.DayOfWeek;
                        switch (dat)
                        {
                            case 1:
                                dayAll[i].showLabel = dayAll[i].date_str.ToString("周一 d号");
                                break;
                            case 2:
                                dayAll[i].showLabel = dayAll[i].date_str.ToString("周二 d号");
                                break;
                            case 3:
                                dayAll[i].showLabel = dayAll[i].date_str.ToString("周三 d号");
                                break;
                            case 4:
                                dayAll[i].showLabel = dayAll[i].date_str.ToString("周四 d号");
                                break;
                            case 5:
                                dayAll[i].showLabel = dayAll[i].date_str.ToString("周五 d号");
                                break;
                            case 6:
                                dayAll[i].showLabel = dayAll[i].date_str.ToString("周六 d号");
                                break;
                            case 0:
                                dayAll[i].showLabel = dayAll[i].date_str.ToString("周天 d号");
                                break;
                        }
                    }
                    else
                    {
                        dayAll = dayAll.OrderByDescending(t => t.date_str).ToList();
                        List<int> hours = new List<int>();
                        foreach (var item in dayAll)
                        {
                            if (!string.IsNullOrEmpty(item.showLabel))
                                continue;
                            if (hours.Contains(item.date_str.Hour))
                                continue;


                            switch (item.date_str.Hour)
                            {
                                case 3:
                                case 6:
                                case 9:
                                case 12:
                                case 15:
                                case 18:
                                case 21:
                                    hours.Add(item.date_str.Hour);
                                    item.showLabel = item.date_str.ToString("HH:00");
                                    break;

                            }
                        }
                        break;
                    }
                }
                idx++;
            }
            return sugers.OrderBy(s => s.date_str).ToList();
        }


        private void FormatDate(EntriesEntity dd)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds((long)dd.date);
            DateTime dateTime = dateTimeOffset.DateTime.AddMinutes(dd.utcOffset);
            dd.date_str = dateTime;
            dd.date_time = dateTime.ToString("HH:mm:ss");
            dd.date_day = dateTime.ToString("yyyy-MM-dd");

            dd.date_step = (int)(DateTime.Now - dateTime).TotalMinutes;

            if (dd.sgv != null)
                dd.sgv_str = Math.Round(1.0 * dd.sgv.Value / 18, 1);
             
            dd.direction_str = _nightscoutServices.GetNsFlag(dd.direction);
        }  
    }

    public class SugarDTO
    {
        public List<string> groupDays { get; set; } = new List<string>();
        public List<string> groupDaysPercent { get; set; } = new List<string>();

        public List<EntriesEntity> day0 { get; set; } = new List<EntriesEntity>();

        public List<EntriesEntity> day1 { get; set; } = new List<EntriesEntity>();

        public List<EntriesEntity> day2 { get; set; } = new List<EntriesEntity>();

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
        public DateTime date_str { get; set; }
        public string date_time { get; set; }
        public string date_day { get; set; }
        public int? date_step { get; set; }
        public double? sgv { get; set; }
        public double? sgv_str { get; set; }
        public string direction { get; set; }
        public string direction_str { get; set; }
        public bool isMask { get; set; }
        public string title { get; set; }
        public string saying { get; set; }
        public double percent { get; set; }
        public double utcOffset { get; set; }
        public string showLabel { get; set; }
        public DateTime? probeStartTime { get; set; }
        public int probeUseDays { get; set; }
        public int probeUseHours { get; set; }
        public int probeUseMinutes { get; set; }
        public double nextRefreshTime { get; set; }
    }
    public class AccessTokenDto
    {
        public string access_token { get; set; }
    }

}
