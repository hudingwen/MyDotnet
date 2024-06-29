using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.ExceptionDomain;
using MyDotnet.Domain.Dto.Guiji;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.Sannuo;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services.System;
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
                    await loginGuardAccount(data);
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
            }
            return MessageModel<bool>.Fail("还未实现");
        }
        /// <summary>
        /// 登录硅基
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
                        throw new ServiceException($"硅基登录失败:{loginRes.msg}");
                    data.token = loginRes.data.access_token;
                    data.tokenExpire = DateTime.Now.AddSeconds(loginRes.data.expires_in);
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
            //登录账户
            if ("100".Equals(data.guardType))
            {
                //硅基
                await loginGuardAccount(data);
            }
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
                whereExpression = whereExpression.And(t => t.name.Contains(key));
            }
            var data = await _baseRepositoryUser.QueryPage(whereExpression, page, size);
            return data;
        }
        /// <summary>
        /// 添加监护者
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<long> addGuardUser(NightscoutGuardUser data)
        {
            data.refreshTime = DateTime.Now;
            //添加
            var i = await _baseRepositoryUser.Add(data);
            //添加api
            var nightscout = await _nightscoutServices.Dal.QueryById(data.nid);
            var server = await _nightscoutServerServices.Dal.QueryById(nightscout.serverId);
            var token = await addToken(data,nightscout, server);
            return i;
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
            var i = await _baseRepositoryUser.DeleteById(user.Id);
            var task = (await _tasksQzServices.Dal.Query(t => t.ResourceId == user.Id)).FirstOrDefault();
            if(task != null)
            {
               await  _tasksQzServices.DeleteTask(task.Id);
            }
           
            return i;
        }
        /// <summary>
        /// 通过api推送血糖
        /// </summary>
        /// <param name="guardUser"></param>
        /// <param name="data"></param>
        public async Task pushBlood(NightscoutGuardUser guardUser,List<NsUploadBloodInfo> data)
        {
            var nightscout = await _nightscoutServices.Dal.QueryById(guardUser.nid);
            var url = $"https://{nightscout.url}/api/v1/entries"; 
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("API-SECRET", guardUser.nsToken);
            request.Content = new StringContent(JsonHelper.ObjToJson(data,false), Encoding.UTF8, "application/json");
            var res = await HttpHelper.SendAsync(request);
            guardUser.refreshTime = DateTimeOffset.FromUnixTimeMilliseconds(data[data.Count - 1].date).UtcDateTime.ToLocalTime();
            await _baseRepositoryUser.Update(guardUser,t=>new {t.refreshTime});
        }


        public async Task<NsApiToken> addToken(NightscoutGuardUser guardUser, Nightscout nightscout,NightscoutServer server, NsApiToken nsApiToken=null)
        {
            var mongoServer = await _nightscoutServerServices.Dal.QueryById(server.mongoServerId);
            
            //创建用户
            var grantConnectionMongoString = $"mongodb://{mongoServer.mongoLoginName}:{mongoServer.mongoLoginPassword}@{mongoServer.mongoIp}:{mongoServer.mongoPort}";
            var client = new MongoClient(grantConnectionMongoString);

            var database = client.GetDatabase(nightscout.serviceName);
            //修改参数
            var collection = database.GetCollection<NsApiToken>("auth_subjects"); // 集合
            
            NsApiToken data = new NsApiToken();
            if(nsApiToken == null)
            {
                data.id = ObjectId.GenerateNewId();
                data.name = "api";
                data.roles = new List<string> { "admin" };
                data.notes = "自动生成请勿删除";
                //data.created_at = "2024-06-20T23:30:32.328Z";
                data.created_at = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                await collection.InsertOneAsync(data);
            }
            else
            {
                data = nsApiToken;
                var tokens = await collection.Find(t => true).ToListAsync();
                bool isAdd = false;
                foreach (var token in tokens)
                {
                    if (token.id.ToString().Equals(nsApiToken.id.ToString()))
                    {
                        //添加过了
                        isAdd = true;
                    }
                }
                if (!isAdd)
                {
                    await collection.InsertOneAsync(data);
                }
                
            }
            //更新
            guardUser.nsTokenId = data.id.ToString();
            guardUser.nsTokenName = data.name;
            guardUser.nsToken = await GetSubjectHash(nightscout.passwd, guardUser.nsTokenId, guardUser.nsTokenName);
            await _baseRepositoryUser.Update(guardUser);
            //重启实例
            await _nightscoutServices.Refresh(nightscout, server);
            return data;
        }
        /// <summary>
        /// 获取ns api的token
        /// </summary>
        /// <param name="apiKey">apiKey</param>
        /// <param name="id">subject的id</param>
        /// <param name="name">subject的name</param>
        /// <returns></returns>
        public async Task<string> GetSubjectHash(string apiKey, string id,string name)
        {
            var apiKeySHA1 = await GenHash(apiKey);
            using (SHA1 sha1 = SHA1.Create())
            {
                // 将秘密键更新到哈希对象中
                byte[] secretBytes = Encoding.UTF8.GetBytes(apiKeySHA1);
                sha1.TransformBlock(secretBytes, 0, secretBytes.Length, secretBytes, 0);

                // 将ID更新到哈希对象中
                byte[] idBytes = Encoding.UTF8.GetBytes(id);
                sha1.TransformFinalBlock(idBytes, 0, idBytes.Length);

                // 生成哈希值并转换为十六进制字符串
                StringBuilder sb = new StringBuilder();
                foreach (byte b in sha1.Hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                await Task.CompletedTask;
                return name + "-" + sb.ToString().ToLower().Substring(0, 16);
            } 
        }
        /// <summary>
        /// 获取内容的hash
        /// </summary>
        /// <param name="data"></param>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<string> GenHash(string data, string algorithm = "sha1")
        {
            using (HashAlgorithm hashAlgorithm = HashAlgorithm.Create(algorithm))
            {
                if (hashAlgorithm == null)
                    throw new ArgumentException("Invalid algorithm specified");

                byte[] byteData = Encoding.UTF8.GetBytes(data);
                byte[] hash = hashAlgorithm.ComputeHash(byteData);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                await Task.CompletedTask;
                return sb.ToString();
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
                var ls = await GuijiHelper.getGuijiList(guardAccount.token, page, size);
                PageModel<ShowKeyValueDto> data = new PageModel<ShowKeyValueDto>();
                data.page = ls.data.currentPage;
                data.dataCount = ls.data.total;
                data.size = ls.data.pageSize;
                data.data = ls.data.records.Select(t => new ShowKeyValueDto { id = t.id, name = $"{t.otherInfo.followedRemark}-{t.followedUserInfo.userName}({t.followedUserInfo.nickName})" }).ToList();
                return data;
            }
            if ("200".Equals(guardAccount.guardType))
            {
                var users = await SannuoHelper.getFamily(guardAccount.token);
                if (users.success)
                {

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
                    data.data = users.data.Select(t=>new ShowKeyValueDto { id= t.sannuoFamilyUserDto.data.encryptUserId ,name=$"{t.nickName}({t.phone})"}).ToList();
                    return data;
                }
                else
                {
                    throw new ServiceException($"获取用户失败:{users.msg}");
                }
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

        public void GetNsFlagForSannuo(List<SannuoBloodDtoData> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                SannuoBloodDtoData curRow = data[i];
                double diff;

                if (data.Count == 1)
                {
                    diff = 0;
                }else 
                {
                    diff = (curRow.value - data[i-1].value) / 3;
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
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.logApp.Error($"账户查询失败:{ex.Message}", ex);
                return false;
            }
        }
    }
}
