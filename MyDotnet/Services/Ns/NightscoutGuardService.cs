using AppStoreConnect.Model;
using Dm.util;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.ExceptionDomain;
using MyDotnet.Domain.Dto.Guard;
using MyDotnet.Domain.Dto.Guiji;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.Outai;
using MyDotnet.Domain.Dto.Sannuo;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Dto.Weitai1;
using MyDotnet.Domain.Dto.Weitai2;
using MyDotnet.Domain.Dto.Yapei;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Helper;
using MyDotnet.Helper.Ns;
using MyDotnet.Repository;
using MyDotnet.Services.System;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace MyDotnet.Services.Ns
{
    /// <summary>
    /// 监护服务
    /// </summary>
    public class NightscoutGuardService : BaseServices<NightscoutGuardUser>
    {
        public BaseRepository<NightscoutGuardUser> _baseRepositoryUser;
        public BaseRepository<NightscoutGuardAccount> _baseRepositoryAccount;
        private UnitOfWorkManage _unitOfWorkManage;
        private NightscoutServices _nightscoutServices;
        public TasksQzServices _tasksQzServices;
        public BaseServices<NightscoutServer> _nightscoutServerServices { get; set; }
        public NightscoutGuardService(BaseRepository<NightscoutGuardUser> baseRepositoryUser
            , BaseRepository<NightscoutGuardAccount> baseRepositoryAccount
            , UnitOfWorkManage unitOfWorkManage
            , NightscoutServices nightscoutServices
            , BaseServices<NightscoutServer> nightscoutServerServices
            , TasksQzServices tasksQzServices)
            : base(baseRepositoryUser)
        {
            _baseRepositoryUser = baseRepositoryUser;
            _baseRepositoryAccount = baseRepositoryAccount;
            _unitOfWorkManage = unitOfWorkManage;
            _nightscoutServices = nightscoutServices;
            _nightscoutServerServices = nightscoutServerServices;
            _tasksQzServices = tasksQzServices;
        }
        /// <summary>
        /// 获取监护账户列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<PageModel<NightscoutGuardAccount>> getGuardAccountList(int page,int size,string key="")
        {
            Expression<Func<NightscoutGuardAccount, bool>> whereExpression = a => true;
            if (!string.IsNullOrEmpty(key))
            {
                whereExpression = whereExpression.And(t => t.name.Contains(key));
            }
            var data = await _baseRepositoryAccount.QueryPage(whereExpression, page, size);
            foreach (var item in data.data)
            {
                item.isEffect = await CheckAccount(item);
            }
            return data;
        }
        /// <summary>
        /// 添加监护账户
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<long> addGuardAccount(NightscoutGuardAccount data)
        {
            try
            {
                _unitOfWorkManage.BeginTran();
                //添加账户
                var i = await _baseRepositoryAccount.Add(data);
                //登录账户
                if ("100".Equals(data.guardType))
                {
                    //硅基
                    var res = await loginGuardAccount(data);
                    if (!res.success) throw new ServiceException(res.msg);
                }else if ("300".Equals(data.guardType))
                {
                    //微泰1
                    var res = await loginGuardAccount(data);
                    if (!res.success) throw new ServiceException(res.msg);
                }
                else if ("400".Equals(data.guardType))
                {
                    //微泰1
                    var res = await loginGuardAccount(data);
                    if (!res.success) throw new ServiceException(res.msg);
                }
                else if ("500".Equals(data.guardType))
                {
                    //欧泰
                    var res = await loginGuardAccount(data);
                    if (!res.success) throw new ServiceException(res.msg);
                }
                else if ("110".Equals(data.guardType))
                {
                    //硅基轻享
                    var res = await loginGuardAccount(data);
                    if (!res.success) throw new ServiceException(res.msg);
                }
                else if ("600".Equals(data.guardType))
                {
                    //雅培
                    var res = await loginGuardAccount(data);
                    if (!res.success) throw new ServiceException(res.msg);
                }
                _unitOfWorkManage.CommitTran();
                return i;
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
            
        }
        /// <summary>
        /// 登录硅基
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="ServiceException"></exception>
        public async Task<MessageModel<bool>> refreshGuardAccount(NightscoutGuardAccount data)
        {
            if("100".Equals(data.guardType))
            {
                //硅基
                return await loginGuardAccount(data);
            }else if ("300".Equals(data.guardType))
            {
                //微泰1
                return await loginGuardAccount(data);
            }else if("400".Equals(data.guardType))
            {
                //微泰2
                return await loginGuardAccount(data);
            }
            else if ("500".Equals(data.guardType))
            {
                //欧泰
                return await loginGuardAccount(data);
            }
            else if ("110".Equals(data.guardType))
            {
                //硅基轻享
                return await loginGuardAccount(data);
            }
            else if ("600".Equals(data.guardType))
            {
                //雅培
                return await loginGuardAccount(data);
            }
            return MessageModel<bool>.Fail("还未实现");
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="ServiceException"></exception>
        public async Task<MessageModel<bool>> loginGuardAccount(NightscoutGuardAccount data)
        {
            try
            {
                if ("100".Equals(data.guardType))
                {
                    //硅基
                    var loginRes = await GuijiHelper.loginGuiji(data.loginName, data.loginPass);
                    if (!loginRes.success)
                        return MessageModel<bool>.Success($"硅基登录失败:{loginRes.msg}");
                    data.token = loginRes.data.access_token;
                    data.tokenExpire = DateTime.Now.AddSeconds(loginRes.data.expires_in);
                    await _baseRepositoryAccount.Update(data);
                    return MessageModel<bool>.Success("登录成功");
                }else if ("300".Equals(data.guardType))
                {
                    //微泰1
                    var loginRes = await Weitai1Helper.Login(data.loginName, data.loginPass);
                    if (!"100000".Equals(loginRes.info.code))
                        return MessageModel<bool>.Success($"微泰1登录失败:{loginRes.info.msg}");
                    data.token = loginRes.token;
                    data.tokenExpire = loginRes.tokenExpire;
                    await _baseRepositoryAccount.Update(data);
                    return MessageModel<bool>.Success("登录成功");
                }
                else if ("400".Equals(data.guardType))
                {
                    //微泰2
                    var loginRes = await Weitai2Helper.Login(data.loginName, data.loginPass);
                    if (loginRes.code != 200)
                        return MessageModel<bool>.Success($"微泰2登录失败:{loginRes.msg}");
                    data.token = loginRes.data.token;
                    data.tokenExpire = loginRes.data.tokenExpire;
                    data.loginId = loginRes.data.userId;
                    await _baseRepositoryAccount.Update(data);
                    return MessageModel<bool>.Success("登录成功");
                }
                else if ("500".Equals(data.guardType))
                {
                    //欧泰
                    var loginRes = await OutaiHelper.Login(data.loginName, data.loginPass);
                    if (loginRes.state != 1)
                        return MessageModel<bool>.Success($"欧泰登录失败:{loginRes.msg}");
                    data.token = loginRes.token;
                    data.tokenExpire = loginRes.tokenExpire; 
                    await _baseRepositoryAccount.Update(data);
                    return MessageModel<bool>.Success("登录成功");
                }
                else if ("110".Equals(data.guardType))
                {
                    //硅基轻享
                    var loginRes = await GuijiLiteHelper.loginGuiji(data.loginName, data.loginPass);
                    if (!loginRes.success)
                        return MessageModel<bool>.Success($"硅基轻享登录失败:{loginRes.msg}");
                    data.token = loginRes.data.access_token;
                    data.tokenExpire = DateTime.Now.AddSeconds(loginRes.data.expires_in);
                    await _baseRepositoryAccount.Update(data);
                    return MessageModel<bool>.Success("登录成功");
                }
                else if ("600".Equals(data.guardType))
                {
                    //雅培
                    var loginRes = await YapeiHelper.Login(data.loginName, data.loginPass, data.loginArea, data.appVersion);
                    if (loginRes.status != 0)
                        return MessageModel<bool>.Success($"雅培登录失败:{loginRes.error.message}");
                    data.token = loginRes.data.authTicket.token;
                    data.tokenExpire = loginRes.data.authTicket.tokenExpireTime;
                    data.loginId = loginRes.data.user.id;
                    await _baseRepositoryAccount.Update(data);
                    return MessageModel<bool>.Success("登录成功");
                }
                return MessageModel<bool>.Fail("还未实现");
            }
            catch (Exception ex)
            {
                return MessageModel<bool>.Fail($"登录失败:{ex.Message}");
            }
        }

        /// <summary>
        /// 编辑监护账户
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> editGuardAccount(NightscoutGuardAccount data)
        {
            var i = await _baseRepositoryAccount.Update(data);
            return i;
        }
        /// <summary>
        /// 删除监护账户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> delGuardAccount(long id)
        {
            var i = await _baseRepositoryAccount.DeleteById(id);
            return i;
        }


        /// <summary>
        /// 获取监护用户列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<PageModel<NightscoutGuardUser>> getGuardUserList(int page, int size, string key="")
        {
            Expression<Func<NightscoutGuardUser, bool>> whereExpression = a => true;
            if (!string.IsNullOrEmpty(key))
            {
                whereExpression = whereExpression.And(t => t.name.Contains(key) 
                || t.gidName.Contains(key) 
                || t.nidName.Contains(key)
                || t.uidName.Contains(key)
                || t.nidUrl.Contains(key));
            }
            var data = await _baseRepositoryUser.QueryPage(whereExpression, page, size,"Id desc");
            return data;
        }
        /// <summary>
        /// 添加监护者
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<long> addGuardUser(NightscoutGuardUser data)
        {

            _unitOfWorkManage.BeginTran();
            try
            {
                data.refreshTime = DateTime.Now;
                //添加
                var i = await _baseRepositoryUser.Add(data);
                //添加api
                var nightscout = await _nightscoutServices.Dal.QueryById(data.nid);
                var server = await _nightscoutServerServices.Dal.QueryById(nightscout.serverId);
                var token = await _nightscoutServices.addToken(nightscout, server);
                _unitOfWorkManage.CommitTran(); 
                return i;
            }
            catch
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
        }
        /// <summary>
        /// 编辑监护者
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> editGuardUser(NightscoutGuardUser data)
        {
            var i = await _baseRepositoryUser.Update(data);
            return i;
        }
        /// <summary>
        /// 删除监护者
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> delGuardUser(long id)
        {
            var user = await _baseRepositoryUser.QueryById(id);
            _unitOfWorkManage.BeginTran();
            try
            {
                var i = await _baseRepositoryUser.DeleteById(user.Id);
                var task = (await _tasksQzServices.Dal.Query(t => t.ResourceId == user.Id)).FirstOrDefault();

                if (task != null)
                {
                    await _tasksQzServices.DeleteTask(task.Id);
                }
                _unitOfWorkManage.CommitTran();
                return i;
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
            
        }
        /// <summary>
        /// 通过api推送血糖
        /// </summary>
        /// <param name="guardUser"></param>
        /// <param name="data"></param>
        public async Task pushBlood(NightscoutGuardUser guardUser,List<NsUploadBloodInfo> data)
        {
            try
            {
                var nightscout = await _nightscoutServices.Dal.QueryById(guardUser.nid);
                var url = $"https://{nightscout.url}/api/v1/entries";
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("API-SECRET", nightscout.nsToken);
                request.Content = new StringContent(JsonHelper.ObjToJson(data, false), Encoding.UTF8, "application/json");
                var res = await HttpHelper.SendAsync(request);
                guardUser.refreshTime = DateTimeOffset.FromUnixTimeMilliseconds(data[data.Count - 1].date).UtcDateTime.ToLocalTime();
                await _baseRepositoryUser.Update(guardUser, t => new { t.refreshTime });
            }
            catch(Exception ex)
            {
                LogHelper.logApp.Error("推送血糖异常,用户可能删除了令牌,请重新添加监护!", ex);
                throw new Exception($"推送血糖异常,用户可能删除了令牌,请重新添加监护!=>错误信息:{ex.Message}");
            }
        }


        /// <summary>
        /// 获取监护账户中的用户
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<PageModel<ShowKeyValueDto>> getAllNsGuardUser(long gid, int page, int size, string key)
        {
            var guardAccount = await _baseRepositoryAccount.QueryById(gid);
            if("100".Equals(guardAccount.guardType))
            {
                //硅基
                var ls = await GuijiHelper.getGuijiList(guardAccount.token, page, size);
                if(ls.code  != 200) throw new ServiceException($"获取用户失败:{ls.msg}");
                PageModel<ShowKeyValueDto> data = new PageModel<ShowKeyValueDto>();
                data.page = ls.data.currentPage;
                data.dataCount = ls.data.total;
                data.size = ls.data.pageSize;
                data.data = ls.data.records.Select(t => new ShowKeyValueDto { id = t.id, name = $"{t.otherInfo.followedRemark}-{t.followedUserInfo.userName}({t.followedUserInfo.nickName})" }).ToList();
                return data;
            }
            else if ("200".Equals(guardAccount.guardType))
            {
                //三诺
                var users = await SannuoHelper.getFamily(guardAccount.token);
                if (!users.success) throw new ServiceException($"获取用户失败:{users.msg}");

                PageModel<ShowKeyValueDto> data = new PageModel<ShowKeyValueDto>();
                if (users.data.Count > 0)
                    users.data.Remove(users.data[users.data.Count - 1]);
                foreach (var user in users.data)
                {
                    //获取用户信息
                    var userInfo = await SannuoHelper.getFamilyUserInfo(guardAccount.token, user.userId);
                    user.sannuoFamilyUserDto = userInfo;
                }
                data.page = 1;
                data.dataCount = users.data.Count;
                data.size = 10;
                data.data = users.data.Select(t => new ShowKeyValueDto { id = t.sannuoFamilyUserDto.data.encryptUserId, name = $"{t.nickName}({t.phone})" }).ToList();
                return data;
            }
            else if ("300".Equals(guardAccount.guardType))
            {
                //微泰1
                var users = await Weitai1Helper.getFamily(guardAccount.token);
                if (!"100000".Equals(users.info.code)) throw new ServiceException($"获取用户失败:{users.info.msg}");

                PageModel<ShowKeyValueDto> data = new PageModel<ShowKeyValueDto>();
                
                
                data.page = 1;
                data.dataCount = users.content.records.Count;
                data.size = 10;
                data.data = users.content.records.Select(t => new ShowKeyValueDto { id = t.id, name = $"{t.userAlias}({t.user.phoneNumber})" }).ToList();
                return data;
            }
            else if ("400".Equals(guardAccount.guardType))
            {
                //微泰2
                var users = await Weitai2Helper.getFamily(guardAccount.token, guardAccount.loginId,page,size);
                if (users.code != 200) throw new ServiceException($"获取用户失败:{users.msg}");

                PageModel<ShowKeyValueDto> data = new PageModel<ShowKeyValueDto>();
                 
                data.page = page;
                data.dataCount = users.count;
                data.size = size;
                data.data = users.data.Select(t => new ShowKeyValueDto { id = t.dataProviderId, name = $"{t.readerAlias}-{t.providerAlias}({t.providerUserName})" }).ToList();
                return data;
            }
            else if ("500".Equals(guardAccount.guardType))
            {
                //欧泰
                var users = await OutaiHelper.getFamily(guardAccount.token, guardAccount.loginName);
                if (users.state != 1) throw new ServiceException($"获取用户失败:{users.msg}");

                PageModel<ShowKeyValueDto> data = new PageModel<ShowKeyValueDto>();

                data.page = 1;
                data.dataCount = users.associateFriendList.Count;
                data.size = 9999;
                data.data = users.associateFriendList.Select(t => new ShowKeyValueDto { id = t.friendUserId, name = $"{t.phone}-{t.remarkName}({t.name})" }).ToList();
                return data;
            }
            else if ("110".Equals(guardAccount.guardType))
            {
                //硅基轻享
                var users = await GuijiLiteHelper.getGuijiList(guardAccount.token,page,size);
                if (!users.success) throw new ServiceException($"获取用户失败:{users.msg}");

                PageModel<ShowKeyValueDto> data = new PageModel<ShowKeyValueDto>();

                data.page = page;
                data.dataCount = users.data.total;
                data.size = size;
                data.data = users.data.records.Select(t => new ShowKeyValueDto { id = t.id, name = $"{t.followedUserInfo.userName}-{t.followedUserInfo.nickName}" }).ToList();
                return data;
            }
            else if ("600".Equals(guardAccount.guardType))
            {
                //雅培
                var users = await YapeiHelper.getFamily(guardAccount.token, guardAccount.loginId, guardAccount.loginArea, guardAccount.appVersion);
                if (users.status != 0) throw new ServiceException($"获取用户失败:{users.error.message}");

                PageModel<ShowKeyValueDto> data = new PageModel<ShowKeyValueDto>();

                data.page = page;
                data.dataCount = users.data.Count;
                data.size = size;
                data.data = users.data.Select(t => new ShowKeyValueDto { id = t.patientId, name = $"{t.firstName} {t.lastName}" }).ToList();
                return data;
            }
            else
            {
                throw new ServiceException("还未实现");
            }
        }

        public string GetNsFlagForGuiji(int direction)
        {
            switch (direction)
            {
                case 4:
                    return "TripleUp";
                case 3:
                    return "DoubleUp";
                case 2:
                    return "SingleUp";
                case 1:
                    return "FortyFiveUp";
                case 0:
                    return "Flat";
                case -1:
                    return "FortyFiveDown";
                case -2:
                    return "SingleDown";
                case -3:
                    return "DoubleDown";
                case -4:
                    return "TripleDown";
                case 1000:
                    return "NOT COMPUTABLE";
                case -1000:
                    return "RATE OUT OF RANGE";
                default:
                    return "NOT COMPUTABLE";
            }
        }

        
        /// <summary>
        /// 检测账户有效性
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task<bool> CheckAccount(NightscoutGuardAccount account)
        {
            try
            {
                if("100".Equals(account.guardType))
                {
                    //硅基
                    var loginInfo = await GuijiHelper.getMyInfo(account.token);
                    return loginInfo.success;
                }
                else if ("200".Equals(account.guardType))
                {
                    //三诺
                    var loginInfo = await SannuoHelper.getMyInfo(account.token);
                    return loginInfo.success;
                }
                else if ("300".Equals(account.guardType))
                {
                    //微泰1
                    var loginInfo = await Weitai1Helper.getMyInfo(account.token);
                    return "100000".Equals(loginInfo.info.code);
                }
                else if ("400".Equals(account.guardType))
                {
                    //微泰2
                    var loginInfo = await Weitai2Helper.getMyInfo(account.token);
                    return loginInfo.code == 200;
                }
                else if ("500".Equals(account.guardType))
                {
                    //微泰2
                    var loginInfo = await OutaiHelper.getMyInfo(account.token, account.loginName);
                    return loginInfo.state == 1;
                }
                else if ("110".Equals(account.guardType))
                {
                    //硅基轻享
                    var loginInfo = await GuijiLiteHelper.getMyInfo(account.token);
                    return loginInfo.success;
                }
                else if ("600".Equals(account.guardType))
                {
                    //雅培
                    var loginInfo = await YapeiHelper.getMyInfo(account.token,account.loginId, account.loginArea, account.appVersion);
                    return loginInfo.status == 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.logApp.Error($"账户查询失败:{ex.Message}", ex);
                return false;
            }
        }
        public void GetNsFlagForSannuo(List<SannuoBloodDtoData> pushData)
        {
            for (int i = 0; i < pushData.Count; i++)
            {
                SannuoBloodDtoData curRow = pushData[i];
                double diff;

                if (i == 0)
                {
                    diff = 0;
                }
                else
                {
                    diff = (curRow.value - pushData[i - 1].value) / 3;
                }


                if (diff > 0.17)
                    curRow.direction = "DoubleUp";
                else if (diff > 0.1 && diff <= 0.17)
                    curRow.direction = "SingleUp";
                else if (diff > 0.05 && diff <= 0.1)
                    curRow.direction = "FortyFiveUp";
                if (diff < -0.17)
                    curRow.direction = "DoubleDown";
                else if (diff < -0.1 && diff >= -0.17)
                    curRow.direction = "SingleDown";
                else if (diff < -0.05 && diff >= -0.1)
                    curRow.direction = "FortyFiveDown";
                else if (diff >= -0.05 && diff <= 0.05)
                    curRow.direction = "Flat";
                else
                    curRow.direction = "Flat";
            }
        }
        public void GetNsFlagForWeitai1(List<Weitai1BloodDtoContentRecord> pushData)
        {
            var data = pushData.OrderBy(t => t.deviceTime).ToList();
            for (int i = 0; i < data.Count; i++)
            {
                Weitai1BloodDtoContentRecord curRow = data[i];
                
                double diff;

                if (i == 0)
                {
                    diff = 0;
                }
                else
                {
                    diff = (curRow.eventData - data[i - 1].eventData) / 5;
                }


                if (diff > 0.17)
                    curRow.direction = "DoubleUp";
                else if (diff > 0.1 && diff <= 0.17)
                    curRow.direction = "SingleUp";
                else if (diff > 0.05 && diff <= 0.1)
                    curRow.direction = "FortyFiveUp";
                if (diff < -0.17)
                    curRow.direction = "DoubleDown";
                else if (diff < -0.1 && diff >= -0.17)
                    curRow.direction = "SingleDown";
                else if (diff < -0.05 && diff >= -0.1)
                    curRow.direction = "FortyFiveDown";
                else if (diff >= -0.05 && diff <= 0.05)
                    curRow.direction = "Flat";
                else
                    curRow.direction = "Flat";
            }
        }

        public void GetNsFlagForWeitai2(List<Weitai2BloodDtoData> pushData)
        {
            var data = pushData.OrderBy(t => t.appCreateTime).ToList();
            for (int i = 0; i < data.Count; i++)
            {
                Weitai2BloodDtoData curRow = data[i];
                double diff;

                if (i == 0)
                {
                    diff = 0;
                }
                else
                {
                    diff = (curRow.glucose - data[i - 1].glucose) / 1 /18;
                }


                if (diff > 0.17)
                    curRow.direction = "DoubleUp";
                else if (diff > 0.1 && diff <= 0.17)
                    curRow.direction = "SingleUp";
                else if (diff > 0.05 && diff <= 0.1)
                    curRow.direction = "FortyFiveUp";
                if (diff < -0.17)
                    curRow.direction = "DoubleDown";
                else if (diff < -0.1 && diff >= -0.17)
                    curRow.direction = "SingleDown";
                else if (diff < -0.05 && diff >= -0.1)
                    curRow.direction = "FortyFiveDown";
                else if (diff >= -0.05 && diff <= 0.05)
                    curRow.direction = "Flat";
                else
                    curRow.direction = "Flat";
            }
        }

        public void GetNsFlagForYapei(YapeiBloodInfo yapeiBlood)
        {
            double diff = 0;
            if(yapeiBlood.data.graphData != null && yapeiBlood.data.graphData.Count != 0)
            {
                diff = yapeiBlood.data.connection.glucoseItem.Value - yapeiBlood.data.graphData[yapeiBlood.data.graphData.Count - 1].Value;
            }
             

            if (diff != 0)
            {
                diff = diff / 18;
            }


            if (diff > 0.17)
                yapeiBlood.data.connection.glucoseItem.direction = "DoubleUp";
            else if (diff > 0.1 && diff <= 0.17)
                yapeiBlood.data.connection.glucoseItem.direction = "SingleUp";
            else if (diff > 0.05 && diff <= 0.1)
                yapeiBlood.data.connection.glucoseItem.direction = "FortyFiveUp";
            if (diff < -0.17)
                yapeiBlood.data.connection.glucoseItem.direction = "DoubleDown";
            else if (diff < -0.1 && diff >= -0.17)
                yapeiBlood.data.connection.glucoseItem.direction = "SingleDown";
            else if (diff < -0.05 && diff >= -0.1)
                yapeiBlood.data.connection.glucoseItem.direction = "FortyFiveDown";
            else if (diff >= -0.05 && diff <= 0.05)
                yapeiBlood.data.connection.glucoseItem.direction = "Flat";
            else
                yapeiBlood.data.connection.glucoseItem.direction = "Flat";
        }

        public void GetNsFlagForOutai(List<OutaiBloodDtoContentRecordItem> pushData)
        {
            var data = pushData.OrderBy(t => t.timeFormat).ToList();
            for (int i = 0; i < data.Count; i++)
            {
                OutaiBloodDtoContentRecordItem curRow = data[i];
                double diff;

                if (i == 0)
                {
                    diff = 0;
                }
                else
                {
                    diff = (curRow.value - data[i - 1].value) / 5;
                }


                if (diff > 0.17)
                    curRow.direction = "DoubleUp";
                else if (diff > 0.1 && diff <= 0.17)
                    curRow.direction = "SingleUp";
                else if (diff > 0.05 && diff <= 0.1)
                    curRow.direction = "FortyFiveUp";
                if (diff < -0.17)
                    curRow.direction = "DoubleDown";
                else if (diff < -0.1 && diff >= -0.17)
                    curRow.direction = "SingleDown";
                else if (diff < -0.05 && diff >= -0.1)
                    curRow.direction = "FortyFiveDown";
                else if (diff >= -0.05 && diff <= 0.05)
                    curRow.direction = "Flat";
                else
                    curRow.direction = "Flat";
            }
        }

        public async Task<List<GuardBloodInfo>> getUserNowBloodList(long guardUserid)
        {
            var user = await _baseRepositoryUser.QueryById(guardUserid);
            var guard = await _baseRepositoryAccount.QueryById(user.gid);
            List<GuardBloodInfo> ls = new List<GuardBloodInfo>();
            if ("100".Equals(guard.guardType))
            {
                //硅基
                var data = await GuijiHelper.getUserBlood(guard.token, user.uid);

                if (data.data.followedDeviceGlucoseDataPO.glucoseInfos != null && data.data.followedDeviceGlucoseDataPO.glucoseInfos.Count > 0 && data.data.followedDeviceGlucoseDataPO.time == data.data.followedDeviceGlucoseDataPO.glucoseInfos[0].time)
                {
                    //正常
                    ls = data.data.followedDeviceGlucoseDataPO.glucoseInfos.OrderByDescending(t => t.time).ToList().Select(t => new GuardBloodInfo { time = t.time, blood = t.v, trend = GetNsFlag(GetNsFlagForGuiji(t.s)) }).ToList();
                }
                else
                {
                    //延期
                    ls.Add(new GuardBloodInfo() { time = data.data.followedDeviceGlucoseDataPO.time, blood = data.data.followedDeviceGlucoseDataPO.latestGlucoseValue, trend = GetNsFlag(GetNsFlagForGuiji(data.data.followedDeviceGlucoseDataPO.bloodGlucoseTrend)) });
                }
            }
            else if ("200".Equals(guard.guardType))
            {
                //三诺
                var data = await SannuoHelper.getUserBlood(guard.token, user.uid);
                GetNsFlagForSannuo(data.data);
                ls = data.data.OrderByDescending(t=>t.time).Select(t => new GuardBloodInfo() { time = t.parsTime,blood = t.value,trend = GetNsFlag(t.direction ) }).ToList();
            }
            else if ("300".Equals(guard.guardType))
            {
                //微泰1
                var data = await Weitai1Helper.getBlood(guard.token, user.uid);
                var pushData = data.content.records.Where(t => t.eventType == 7).OrderByDescending(t => t.deviceTime).ToList();
                GetNsFlagForWeitai1(pushData);
                ls = pushData.Select(t => new GuardBloodInfo() { time =t.deviceTime,blood = t.eventData ,trend = GetNsFlag(t.direction)}).ToList();
            }
            else if ("400".Equals(guard.guardType))
            {
                //微泰2
                var data = await Weitai2Helper.getBlood(guard.token, user.uid);
                var pushData = data.data.OrderByDescending(t => t.appCreateTime).ToList();
                //趋势计算
                GetNsFlagForWeitai2(pushData);
                ls = pushData.Select(t => new GuardBloodInfo() { time = t.appCreateTime, blood = Math.Round(t.glucose / 18, 1), trend = GetNsFlag(t.direction) }).ToList();
            }
            else if ("500".Equals(guard.guardType))
            {
                //欧泰
                var data = await OutaiHelper.getBlood(guard.token, guard.loginName, user.uid);
                var pushData = data.content.bloodSugarRecords.records.OrderByDescending(t => t.timeFormat).ToList();
                //趋势计算
                GetNsFlagForOutai(pushData);
                ls = pushData.Select(t => new GuardBloodInfo() { time = t.timeFormat, blood = t.value, trend = GetNsFlag(t.direction) }).ToList();
            }else if ("110".Equals(guard.guardType))
            {
                //硅基轻享
                var data = await GuijiLiteHelper.getUserBlood(guard.token, user.uid);

                if (data.data.followedDeviceGlucoseDataPO.glucoseInfos != null && data.data.followedDeviceGlucoseDataPO.glucoseInfos.Count > 0 && data.data.followedDeviceGlucoseDataPO.time == data.data.followedDeviceGlucoseDataPO.glucoseInfos[0].time)
                {
                    //正常
                    ls = data.data.followedDeviceGlucoseDataPO.glucoseInfos.OrderByDescending(t => t.time).ToList().Select(t => new GuardBloodInfo { time = t.time, blood = t.v, trend = GetNsFlag(GetNsFlagForGuiji(t.s)) }).ToList();
                }
                else
                {
                    //延期
                    ls.Add(new GuardBloodInfo() { time = data.data.followedDeviceGlucoseDataPO.time, blood = data.data.followedDeviceGlucoseDataPO.latestGlucoseValue, trend = GetNsFlag(GetNsFlagForGuiji(data.data.followedDeviceGlucoseDataPO.bloodGlucoseTrend)) });
                }
            }
            else if ("600".Equals(guard.guardType))
            {
                //雅培
                var data = await YapeiHelper.getBlood(guard.token, guard.loginId, user.uid, guard.loginArea, guard.appVersion);
                 
                GetNsFlagForYapei(data);

                ls.Add(new GuardBloodInfo() { time = data.data.connection.glucoseItem.time , blood = Math.Round(data.data.connection.glucoseItem.Value / ("cn".equals(guard.loginArea) ? 1 : 18), 1),trend = GetNsFlag(data.data.connection.glucoseItem.direction) });
                
            }
            return ls;
        }
        public string GetNsFlag(string direction)
        {
            switch (direction)
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
}
