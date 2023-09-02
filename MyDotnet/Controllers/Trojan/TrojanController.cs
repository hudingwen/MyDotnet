using System.Data;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Dto.Trojan;
using MyDotnet.Domain.Entity.Trojan;
using MyDotnet.Helper;
using MyDotnet.Services;

namespace MyDotnet.Controllers.Trojan
{
    /// <summary>
    /// trojan管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class TrojanController : Controller
    {
        public BaseServices<TrojanUsers> _trojanUsersServices;
        public BaseServices<TrojanServers> _baseServicesServers;
        public BaseServices<TrojanDetails> _baseServicesDetails;
        public BaseServices<TrojanCusServers> _baseServicesCusServers;
        public BaseServices<TrojanUrlServers> _baseServicesUrlServers;
        public AspNetUser _user;

        public TrojanController(BaseServices<TrojanUsers> trojanUsersServices
            , BaseServices<TrojanServers> baseServicesServers
            , BaseServices<TrojanDetails> baseServicesDetails
            , BaseServices<TrojanCusServers> baseServicesCusServers
            , BaseServices<TrojanUrlServers> baseServicesUrlServers
            , AspNetUser user
            )
        {
            _trojanUsersServices = trojanUsersServices;
            _baseServicesServers = baseServicesServers;
            _baseServicesDetails = baseServicesDetails;
            _baseServicesCusServers = baseServicesCusServers;
            _baseServicesUrlServers = baseServicesUrlServers;
            _user = user;
        }


        /// <summary>
        /// 获取Trojan用户
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="name"></param>
        /// <param name="isuse"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<TrojanUsers>>> GetUser([FromQuery] PaginationModel pagination, [FromQuery] string name, [FromQuery] bool isuse)
        {
            var whereFind = LinqHelper.True<TrojanUsers>();
            if (!string.IsNullOrEmpty(name))
                whereFind = whereFind.And(t => t.username.Contains(name));
            if (isuse)
                whereFind = whereFind.And(t => t.upload > 0 || t.download > 0);
            var data = await _trojanUsersServices.Dal.QueryPage(whereFind, pagination.PageIndex, pagination.PageSize);
            if (data.data.Count > 0)
            {
                var ids = data.data.Select(t => t.id).ToList();
                var where = LinqHelper.True<TrojanDetails>();
                where = where.And(t => ids.Contains(t.userId));//.And(t => t.calDate < DateTime.Now).And(t => t.calDate > DateTime.Now.AddMonths(-12));
                var userDetails = await _baseServicesDetails.Dal.Query(where);
                foreach (var trojanUser in data.data)
                {
                    var ls = from t in userDetails
                             where t.userId == trojanUser.id
                             group t by new { moth = t.calDate.ToString("yyyy-MM"), id = t.userId } into g
                             orderby g.Key.moth descending
                             select new TrojanUseDetailDto { userId = g.Key.id, moth = g.Key.moth, up = g.Sum(t => Convert.ToDecimal(t.upload)), down = g.Sum(t => Convert.ToDecimal(t.download)) };
                    var lsData = ls.ToList();
                    lsData.Insert(0, new TrojanUseDetailDto { userId = trojanUser.id, up = trojanUser.upload, down = trojanUser.download, moth = DateTime.Now.ToString("yyyy-MM") });
                    trojanUser.useList = lsData;
                }
            }
            return MessageModel<PageModel<TrojanUsers>>.Success("获取成功", data);
        }

        /// <summary>
        /// 获取Trojan用户-下拉列表用
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<DataTable>> GetAllTrojanUser()
        {
            var data = await _trojanUsersServices.Dal.QuerySqlTable("select id,username from users");
            return MessageModel<DataTable>.Success("获取成功", data);
        }
        /// <summary>
        /// 添加Trojan用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<object>> AddUser([FromBody] TrojanUsers user)
        {
            var find = await _trojanUsersServices.Dal.Query(t => t.username == user.username);
            if (find != null && find.Count > 0) return MessageModel<object>.Fail("用户名已存在");
            var pass = StringHelper.GetGUID();
            var passEcrypt = ShaHelper.Sha224(pass);
            //user.quota = 0;
            user.upload = 0;
            user.download = 0;
            user.password = passEcrypt;
            user.passwordshow = pass;
            var data = await _trojanUsersServices.Dal.Db.Insertable<TrojanUsers>(user).ExecuteCommandAsync();
            return MessageModel<object>.Success("添加成功", data);
        }
        /// <summary>
        /// 更新Trojan用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<object>> UpdateUser([FromBody] TrojanUsers user)
        {
            var find = await _trojanUsersServices.Dal.QueryById(user.id);
            if (find == null) return MessageModel<object>.Fail("用户名不存在");
            find.username = user.username;
            var data = await _trojanUsersServices.Dal.Update(find, t => new { t.username });
            return MessageModel<object>.Success("更新成功", data);
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> DelUser([FromBody] int[] users)
        {
            var data = await _trojanUsersServices.Dal.Query(t => users.Contains(t.id));
            var list = data.Select(t => t.id.ToString()).ToArray();
            await _trojanUsersServices.Dal.DeleteByIds(list);
            return MessageModel<string>.Success("删除成功");
        }
        /// <summary>
        /// 重置流量
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> ResetFlow([FromBody] int[] users)
        {
            var data = await _trojanUsersServices.Dal.Query(t => users.Contains(t.id));
            foreach (var item in data)
            {
                item.upload = 0;
                item.download = 0;
                await _trojanUsersServices.Dal.Update(item, t => new { t.upload, t.download });
            }
            return MessageModel<string>.Success("重置流量成功");
        }
        /// <summary>
        /// 限制流量
        /// </summary>
        /// <param name="limit"></param> 
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> LimitFlow([FromBody] TrojanLimitFlowDto limit)
        {
            var data = await _trojanUsersServices.Dal.Query(t => limit.users.Contains(t.id));
            foreach (var item in data)
            {
                item.quota = limit.quota;
                await _trojanUsersServices.Dal.Update(item, t => new { t.quota });
            }
            return MessageModel<string>.Success("限制流量成功");
        }
        /// <summary>
        /// 重置链接密码
        /// </summary>
        /// <param name="users"></param> 
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> ResetPass([FromBody] int[] users)
        {
            var data = await _trojanUsersServices.Dal.Query(t => users.Contains(t.id));
            var pass = StringHelper.GetGUID();
            var passEcrypt = ShaHelper.Sha224(pass);
            foreach (var item in data)
            {
                item.password = passEcrypt;
                item.passwordshow = pass;
                await _trojanUsersServices.Dal.Update(item, t => new { t.password, t.passwordshow });
            }
            return MessageModel<string>.Success("重置链接密码成功");
        }
        /// <summary>
        /// 获取Trojan服务器
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        public async Task<MessageModel<List<TrojanServers>>> GetServers()
        {
            var data = await _baseServicesServers.Dal.Query();
            data = data.OrderBy(t => t.servername).ToList();
            return MessageModel<List<TrojanServers>>.Success("获取成功", data);
        }
        /// <summary>
        /// 获取拼接后的Trojan服务器
        /// </summary>
        /// <param name="id">passwordshow</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<TrojanServerSpliceDto>> GetSpliceServers(string id)
        {
            var data = await _baseServicesServers.Dal.Query();
            data = data.OrderBy(t => t.servername).ToList();
            var res = new TrojanServerSpliceDto();
            res.normalApi = ConfigHelper.GetValue(new string[] { "trojan", "normalApi" }).ObjToString();
            res.clashApi = ConfigHelper.GetValue(new string[] { "trojan", "clashApi" }).ObjToString();
            res.clashApiBackup = ConfigHelper.GetValue(new string[] { "trojan", "clashApiBackup" }).ObjToString();
            foreach (var item in data)
            {
                var serverSplice = GetSplice(item, id);
                res.list.Add(new TrojanServerDto { name = item.servername, value = serverSplice });
            }
            return MessageModel<TrojanServerSpliceDto>.Success("获取成功", res); ;

        }
        /// <summary>
        /// 删除Trojan服务器
        /// </summary>
        /// <param name="servers"></param>
        /// <returns></returns> 
        [HttpPut]
        public async Task<MessageModel<List<TrojanServers>>> DelServers([FromBody] long[] servers)
        {
            var data = await _baseServicesServers.Dal.DeleteByIds(servers.Select(t => t.ToString()).ToArray());
            if (data)
                return MessageModel<List<TrojanServers>>.Success("删除成功");
            else
                return MessageModel<List<TrojanServers>>.Fail("删除失败");
        }
        /// <summary>
        /// 更新Trojan服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns> 
        [HttpPut]
        public async Task<MessageModel<List<TrojanServers>>> UpdateServers(TrojanServers server)
        {
            var data = await _baseServicesServers.Dal.Update(server);
            return MessageModel<List<TrojanServers>>.Success("更新成功");
        }
        /// <summary>
        /// 添加Trojan服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns> 
        [HttpPost]
        public async Task<MessageModel<List<TrojanServers>>> AddServers(TrojanServers server)
        {
            var data = await _baseServicesServers.Dal.Add(server);
            return MessageModel<List<TrojanServers>>.Success("添加成功");
        }

        /// <summary>
        /// 获取Cus服务器
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        public async Task<MessageModel<List<TrojanCusServers>>> GetCusServers()
        {
            var data = await _baseServicesCusServers.Dal.Query();
            data = data.OrderBy(t => t.servername).ToList();
            return MessageModel<List<TrojanCusServers>>.Success("获取成功", data);
        }
        /// <summary>
        /// 删除Cus服务器
        /// </summary>
        /// <param name="servers"></param>
        /// <returns></returns> 
        [HttpPut]
        public async Task<MessageModel<List<TrojanCusServers>>> DelCusServers([FromBody] long[] servers)
        {
            var data = await _baseServicesCusServers.Dal.DeleteByIds(servers.Select(t => t.ToString()).ToArray());
            if (data)
                return MessageModel<List<TrojanCusServers>>.Success("删除成功");
            else
                return MessageModel<List<TrojanCusServers>>.Fail("删除失败");
        }
        /// <summary>
        /// 更新Cus服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns> 
        [HttpPut]
        public async Task<MessageModel<List<TrojanCusServers>>> UpdateCusServers(TrojanCusServers server)
        {
            var data = await _baseServicesCusServers.Dal.Update(server);
            return MessageModel<List<TrojanCusServers>>.Success("更新成功");
        }
        /// <summary>
        /// 添加Cus服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns> 
        [HttpPost]
        public async Task<MessageModel<List<TrojanCusServers>>> AddCusServers(TrojanCusServers server)
        {
            var data = await _baseServicesCusServers.Dal.Add(server);
            return MessageModel<List<TrojanCusServers>>.Success("添加成功");
        }


        /// <summary>
        /// 获取Url服务器
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        public async Task<MessageModel<List<TrojanUrlServers>>> GetUrlServers()
        {
            var data = await _baseServicesUrlServers.Dal.Query();
            data = data.OrderBy(t => t.servername).ToList();
            return MessageModel<List<TrojanUrlServers>>.Success("获取成功", data);
        }
        /// <summary>
        /// 删除Url服务器
        /// </summary>
        /// <param name="servers"></param>
        /// <returns></returns> 
        [HttpPut]
        public async Task<MessageModel<List<TrojanUrlServers>>> DelUrlServers([FromBody] long[] servers)
        {
            var data = await _baseServicesUrlServers.Dal.DeleteByIds(servers.Select(t => t.ToString()).ToArray());
            if (data)
                return MessageModel<List<TrojanUrlServers>>.Success("删除成功");
            else
                return MessageModel<List<TrojanUrlServers>>.Fail("删除失败");
        }
        /// <summary>
        /// 更新Url服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns> 
        [HttpPut]
        public async Task<MessageModel<List<TrojanUrlServers>>> UpdateUrlServers(TrojanUrlServers server)
        {
            var data = await _baseServicesUrlServers.Dal.Update(server);
            return MessageModel<List<TrojanUrlServers>>.Success("更新成功");
        }
        /// <summary>
        /// 添加Url服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns> 
        [HttpPost]
        public async Task<MessageModel<List<TrojanUrlServers>>> AddUrlServers(TrojanUrlServers server)
        {
            var data = await _baseServicesUrlServers.Dal.Add(server);
            return MessageModel<List<TrojanUrlServers>>.Success("添加成功");
        }
        private string GetSplice(TrojanServers item, string passwordshow)
        {
            var sni = string.IsNullOrEmpty(item.serverpeer) ? item.serveraddress : item.serverpeer;
            if ("0".Equals(item.servertype))
                return $"trojan://{passwordshow}@{item.serveraddress}:{item.serverport}?allowinsecure=0&tfo=0&fp=chrome&peer={sni}&host={sni}&sni={sni}#{item.servername}";
            else if ("1".Equals(item.servertype))
            {
                return $"trojan://{passwordshow}@{item.serveraddress}:{item.serverport}?wspath={item.serverpath}&ws=1&peer={sni}&path={item.serverpath}&host={sni}&fp=chrome&type=ws&sni={sni}#{item.servername}";
            }
            else
                return $"servertype:({item.servertype})错误";
        }
        private List<string> GetSplice(List<TrojanServers> items, string passwordshow)
        {
            List<string> ls = new List<string>();
            foreach (var item in items)
            {
                ls.Add(GetSplice(item, passwordshow));
            }
            return ls;
        }
        /// <summary>
        /// 获取订阅数据
        /// </summary>
        /// <param name="id">链接密码</param>
        /// <param name="isUseBase64">是否使用base64加密</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<string> RSS(string id, bool isUseBase64 = true)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                var user = (await _trojanUsersServices.Dal.Query(t => t.passwordshow == id)).FirstOrDefault();
                if (user == null) throw new Exception("用户不存在");
                var data = await _baseServicesServers.Dal.Query(t => (t.userid == user.id || t.userid <= 0) && t.serverenable);
                if (data != null)
                {
                    data = data.OrderBy(t => t.servername).ToList();
                    foreach (var item in data)
                    {
                        sb.AppendLine(GetSplice(item, user.passwordshow));
                    }
                }
                var cusData = await _baseServicesCusServers.Dal.Query(t => (t.userid == user.id || t.userid <= 0) && t.serverenable);
                if (cusData != null)
                {
                    cusData = cusData.OrderBy(t => t.servername).ToList();
                    foreach (var item in cusData)
                    {
                        sb.AppendLine(item.serveraddress);
                    }
                }
                var urlData = await _baseServicesUrlServers.Dal.Query(t => (t.userid == user.id || t.userid <= 0) && t.serverenable);
                if (urlData != null)
                {
                    urlData = urlData.OrderBy(t => t.servername).ToList();
                    foreach (var item in urlData)
                    {
                        try
                        {
                            var urlStrObj = await HttpHelper.GetAsync(item.serveraddress);
                            var lines = "";
                            try
                            {
                                lines = Encoding.UTF8.GetString(Convert.FromBase64String(urlStrObj));
                            }
                            catch (Exception)
                            {
                                lines = urlStrObj;
                            }
                            finally
                            {
                                sb.AppendLine(lines);
                            }
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"trojan://xxxxxx@xxxxxx.xx:443?allowinsecure=0&tfo=0#{ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"trojan://xxxxxx@xxxxxx.xx:443?allowinsecure=0&tfo=0#{ex.Message}");
            }
            if (isUseBase64)
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
            }
            else
            {
                return sb.ToString();
            }
        }
    }
}