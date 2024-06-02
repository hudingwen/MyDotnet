using System.Data;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Dto.Trojan;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Domain.Entity.Trojan;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using MyDotnet.Services.System;
using Renci.SshNet;

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
        public BaseServices<TrojanDetails> _baseServicesDetails;
        public BaseServices<TrojanServers> _baseServicesServers;
        public BaseServices<TrojanCusServers> _baseServicesCusServers;
        public BaseServices<TrojanUrlServers> _baseServicesUrlServers;
        public AspNetUser _user;
        public DicService _dicService;
        public BaseServices<TrojanServersUsers> _baseServicesServersUsers;
        public BaseServices<TrojanServersUsersExclude> _baseServicesServersUsersExclude;
        public BaseServices<TrojanCusServersUsers> _baseServicesCusServersUsers;
        public BaseServices<TrojanUrlServersUsers> _baseServicesUrlServersUsers;
        public UnitOfWorkManage _unitOfWorkManage { get; set; }

        public TrojanController(BaseServices<TrojanUsers> trojanUsersServices
            , BaseServices<TrojanServers> baseServicesServers
            , BaseServices<TrojanDetails> baseServicesDetails
            , BaseServices<TrojanCusServers> baseServicesCusServers
            , BaseServices<TrojanUrlServers> baseServicesUrlServers
            , AspNetUser user
            , DicService dicService
            , BaseServices<TrojanServersUsers> baseServicesServersUsers
            , BaseServices<TrojanCusServersUsers> baseServicesCusServersUsers
            , BaseServices<TrojanUrlServersUsers> baseServicesUrlServersUsers
            , UnitOfWorkManage unitOfWorkManage
            , BaseServices<TrojanServersUsersExclude> baseServicesServersUsersExclude
            )
        {
            _trojanUsersServices = trojanUsersServices;
            _baseServicesServers = baseServicesServers;
            _baseServicesDetails = baseServicesDetails;
            _baseServicesCusServers = baseServicesCusServers;
            _baseServicesUrlServers = baseServicesUrlServers;
            _user = user;
            _dicService = dicService;
            _baseServicesServersUsers = baseServicesServersUsers;
            _baseServicesCusServersUsers = baseServicesCusServersUsers;
            _baseServicesUrlServersUsers = baseServicesUrlServersUsers;
            _unitOfWorkManage = unitOfWorkManage;
            _baseServicesServersUsersExclude = baseServicesServersUsersExclude;
        }


        /// <summary>
        /// 获取Trojan用户
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="key"></param>
        /// <param name="isuse"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<TrojanUsers>>> GetUser([FromQuery] PaginationModel pagination, [FromQuery] string key, [FromQuery] bool isuse)
        {
            var whereFind = LinqHelper.True<TrojanUsers>();
            if (!string.IsNullOrEmpty(key))
                whereFind = whereFind.And(t => t.username.Contains(key));
            if (isuse)
                whereFind = whereFind.And(t => t.upload > 0 || t.download > 0);
            var data = await _trojanUsersServices.Dal.QueryPage(whereFind, pagination.page, pagination.size,"id desc");
            if (data.data.Count > 0)
            {
                //流量统计
                var ids = data.data.Select(t => t.id).ToList();
                var where = LinqHelper.True<TrojanDetails>();
                where = where.And(t => ids.Contains(t.userId));
                var userDetails = await _baseServicesDetails.Dal.Query(where);
                
                //获取绑定服务器
                var whereServerUser = LinqHelper.True<TrojanServersUsers>();
                whereServerUser = whereServerUser.And(t => ids.Contains(t.userid));
                var serverUser = await _baseServicesServersUsers.Dal.Query(whereServerUser);
                //获取排除服务器
                var whereServerUserExclude = LinqHelper.True<TrojanServersUsersExclude>();
                whereServerUserExclude = whereServerUserExclude.And(t => ids.Contains(t.userid));
                var serverUserExclude = await _baseServicesServersUsersExclude.Dal.Query(whereServerUserExclude);


                foreach (var trojanUser in data.data)
                {
                    //流量统计
                    var ls = from t in userDetails
                             where t.userId == trojanUser.id
                             group t by new { moth = t.calDate.ToString("yyyy-MM"), id = t.userId } into g
                             orderby g.Key.moth descending
                             select new TrojanUseDetailDto { userId = g.Key.id, moth = g.Key.moth, up = g.Sum(t => Convert.ToDecimal(t.upload)), down = g.Sum(t => Convert.ToDecimal(t.download)) };
                    var lsData = ls.ToList();
                    lsData.Insert(0, new TrojanUseDetailDto { userId = trojanUser.id, up = trojanUser.upload, down = trojanUser.download, moth = DateTime.Now.ToString("yyyy-MM") });
                    trojanUser.useList = lsData;

                    //绑定服务器
                    trojanUser.serverIds = serverUser.FindAll(t => t.userid == trojanUser.id).Select(t => t.serverid).ToList();
                    //排除服务器
                    trojanUser.serverIdsExclude = serverUserExclude.FindAll(t => t.userid == trojanUser.id).Select(t => t.serverid).ToList();

                }


            }
            return MessageModel<PageModel<TrojanUsers>>.Success("获取成功", data);
        }

        /// <summary>
        /// 获取Trojan用户-下拉列表用
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<object>> GetAllTrojanUser()
        {
            var data = await _trojanUsersServices.Dal.Db.Queryable<TrojanUsers>().OrderByDescending(t => t.id).Select(t => new { t.id, t.username }).ToListAsync();
            return MessageModel<object>.Success("获取成功", data);
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

           
            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _trojanUsersServices.Dal.Db.Insertable<TrojanUsers>(user).ExecuteCommandIdentityIntoEntityAsync();
                //绑定服务器
                List<TrojanServersUsers> trojanServersUsers = new List<TrojanServersUsers>();
                user.serverIds.ForEach(t =>
                {
                    trojanServersUsers.Add(new TrojanServersUsers { serverid = t, userid = user.id });
                });

                await _baseServicesServersUsers.Dal.Delete(t => t.userid == user.id);
                await _baseServicesServersUsers.Dal.Add(trojanServersUsers);

                //排除服务器
                List<TrojanServersUsersExclude> trojanServersUsersExcludes = new List<TrojanServersUsersExclude>();
                user.serverIdsExclude.ForEach(t =>
                {
                    trojanServersUsersExcludes.Add(new TrojanServersUsersExclude { serverid = t, userid = user.id });
                });
                await _baseServicesServersUsersExclude.Dal.Delete(t => t.userid == user.id);
                await _baseServicesServersUsersExclude.Dal.Add(trojanServersUsersExcludes);

                _unitOfWorkManage.CommitTran();
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }

            return MessageModel<object>.Success("添加成功");
        }
        /// <summary>
        /// 更新Trojan用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<object>> UpdateUser([FromBody] TrojanUsers user)
        {
            
            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _trojanUsersServices.Dal.Update(user);
                //绑定服务器
                List<TrojanServersUsers> trojanServersUsers = new List<TrojanServersUsers>();
                user.serverIds.ForEach(t =>
                {
                    trojanServersUsers.Add(new TrojanServersUsers { serverid = t, userid = user.id });
                });
                await _baseServicesServersUsers.Dal.Delete(t => t.userid == user.id);
                await _baseServicesServersUsers.Dal.Add(trojanServersUsers);

                //排除服务器
                List<TrojanServersUsersExclude> trojanServersUsersExcludes = new List<TrojanServersUsersExclude>();
                user.serverIdsExclude.ForEach(t =>
                {
                    trojanServersUsersExcludes.Add(new TrojanServersUsersExclude { serverid = t, userid = user.id });
                });
                await _baseServicesServersUsersExclude.Dal.Delete(t => t.userid == user.id);
                await _baseServicesServersUsersExclude.Dal.Add(trojanServersUsersExcludes);

                _unitOfWorkManage.CommitTran();
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
            //重启服务
            var servers = await _dicService.GetDicData(TrojanInfo.TrojanServer);
            bool isRestartOk = true;
            Exception err = null;
            DicData errDic = null;
            foreach (var item in servers)
            {
                try
                {
                    using (var sshClient = new SshClient(item.code, item.content.ObjToInt(), item.content2, item.content3))
                    {
                        //创建SSH
                        sshClient.Connect();
                        using (var cmd = sshClient.CreateCommand(""))
                        {
                            var res = cmd.Execute($"systemctl restart trojan");
                        }
                        sshClient.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    err = ex;
                    errDic = item;
                    isRestartOk = false;
                    LogHelper.logApp.Error("服务器重启失败", ex);
                }
            }

            return MessageModel<object>.Success($"更新成功{(isRestartOk == false ? ",但有服务器(" + errDic.name + ")重启失败:"+ err.Message : "")}");
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<MessageModel<string>> DelUser(int id)
        {


            try
            {
                _unitOfWorkManage.BeginTran();
                await _trojanUsersServices.Dal.DeleteById(id);
                //绑定服务器 
                await _baseServicesServersUsers.Dal.Delete(t => t.userid == id);

                //排除服务器
                await _baseServicesServersUsersExclude.Dal.Delete(t => t.userid == id);

                _unitOfWorkManage.CommitTran();
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }

            //重启服务
            var servers = await _dicService.GetDicData(TrojanInfo.TrojanServer);
            bool isRestartOk = true;
            Exception err = null;
            DicData errDic = null;
            foreach (var item in servers)
            {
                try
                {
                    using (var sshClient = new SshClient(item.code, item.content.ObjToInt(), item.content2, item.content3))
                    {
                        //创建SSH
                        sshClient.Connect();
                        using (var cmd = sshClient.CreateCommand(""))
                        {
                            var res = cmd.Execute($"systemctl restart trojan");
                        }
                        sshClient.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    err = ex;
                    errDic = item;
                    isRestartOk = false;
                    LogHelper.logApp.Error("服务器重启失败", ex);
                }
            }

            return MessageModel<string>.Success($"删除成功{(isRestartOk == false ? ",但有服务器(" + errDic.name + ")重启失败:" + err.Message : "")}");
        }
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        // DELETE: api/ApiWithActions/5
        [HttpPost]
        public async Task<MessageModel<string>> DelUsers([FromBody] object[] ids)
        {
           

            try
            {
                _unitOfWorkManage.BeginTran();
                await _trojanUsersServices.Dal.DeleteByIds(ids);
                //绑定服务器 
                await _baseServicesServersUsers.Dal.Delete(t => ids.Contains(t.userid));

                //排除服务器
                await _baseServicesServersUsersExclude.Dal.Delete(t => ids.Contains(t.userid));

                _unitOfWorkManage.CommitTran();
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }

            //重启服务
            var servers = await _dicService.GetDicData(TrojanInfo.TrojanServer);
            bool isRestartOk = true;
            Exception err = null;
            DicData errDic = null;
            foreach (var item in servers)
            {
                try
                {
                    using (var sshClient = new SshClient(item.code, item.content.ObjToInt(), item.content2, item.content3))
                    {
                        //创建SSH
                        sshClient.Connect();
                        using (var cmd = sshClient.CreateCommand(""))
                        {
                            var res = cmd.Execute($"systemctl restart trojan");
                        }
                        sshClient.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    err = ex;
                    errDic = item;
                    isRestartOk = false;
                    LogHelper.logApp.Error("服务器重启失败", ex);
                }
            }

            return MessageModel<string>.Success($"删除成功{(isRestartOk == false ? ",但有服务器(" + errDic.name + ")重启失败:" + err.Message : "")}");
        }


        /// <summary>
        /// 重置流量
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> ResetFlow([FromBody] object[] users)
        {
            
            var data = await _trojanUsersServices.Dal.QueryByIDs(users);
            foreach (var item in data)
            {
                item.upload = 0;
                item.download = 0;
                await _trojanUsersServices.Dal.Update(item, t => new { t.upload, t.download });
            }
            //重启服务
            var servers = await _dicService.GetDicData(TrojanInfo.TrojanServer);
            bool isRestartOk = true;
            Exception err = null;
            DicData errDic = null;
            foreach (var item in servers)
            {
                try
                {
                    using (var sshClient = new SshClient(item.code, item.content.ObjToInt(), item.content2, item.content3))
                    {
                        //创建SSH
                        sshClient.Connect();
                        using (var cmd = sshClient.CreateCommand(""))
                        {
                            var res = cmd.Execute($"systemctl restart trojan");
                        }
                        sshClient.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    err = ex;
                    errDic = item;
                    isRestartOk = false;
                    LogHelper.logApp.Error("服务器重启失败", ex);
                }
            }

            return MessageModel<string>.Success($"重置成功{(isRestartOk == false ? ",但有服务器(" + errDic.name + ")重启失败:" + err.Message : "")}");
        }
        /// <summary>
        /// 重置链接密码
        /// </summary>
        /// <param name="users"></param> 
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> ResetPass([FromBody] object[] users)
        {
            var data = await _trojanUsersServices.Dal.QueryByIDs(users);
            var pass = StringHelper.GetGUID();
            var passEcrypt = ShaHelper.Sha224(pass);
            foreach (var item in data)
            {
                item.password = passEcrypt;
                item.passwordshow = pass;
                await _trojanUsersServices.Dal.Update(item, t => new { t.password, t.passwordshow });
            }
            //重启服务
            var servers = await _dicService.GetDicData(TrojanInfo.TrojanServer);
            foreach (var item in servers)
            {
                using (var sshClient = new SshClient(item.code, item.content.ObjToInt(), item.content2, item.content3))
                {
                    //创建SSH
                    sshClient.Connect();
                    using (var cmd = sshClient.CreateCommand(""))
                    {
                        var res = cmd.Execute($"systemctl restart trojan");
                    }
                    sshClient.Disconnect();
                }
            }
            return MessageModel<string>.Success("重置链接密码成功");
        }
        /// <summary>
        /// 获取拼接后的Trojan服务器
        /// </summary>
        /// <param name="id">passwordshow</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<TrojanServerSpliceDto>> GetSpliceServers(string id)
        {
            var user = await _trojanUsersServices.Dal.QueryById(id);
            if (user == null) return MessageModel<TrojanServerSpliceDto>.Fail("用户不存在");

            var bindUsers = await _baseServicesServersUsers.Dal.Db.Queryable<TrojanServersUsers>().Where(t => t.userid == user.id).Select(t => t.serverid).ToListAsync();
            var data = await _baseServicesServers.Dal.Query(t => t.serverenable && (t.isAllUser || bindUsers.Contains(t.id)));
            data = data.OrderBy(t => t.servername).ToList();

            var res = new TrojanServerSpliceDto();
            var trojanKeys = await _dicService.GetDicData(TrojanInfo.KEY);

            res.normalApi = trojanKeys.Find(t => t.code == TrojanInfo.KEY_normalApi).content;
            res.clashApi = trojanKeys.Find(t => t.code == TrojanInfo.KEY_clashApi).content;
            res.clashApi2 = trojanKeys.Find(t => t.code == TrojanInfo.KEY_clashApi_v2).content;

            foreach (var item in data)
            {
                var serverSplice = GetSplice(item, user.passwordshow);
                res.list.Add(new TrojanServerDto { name = item.servername, value = serverSplice });
            }
            return MessageModel<TrojanServerSpliceDto>.Success("获取成功", res); ;

        }

        /// <summary>
        /// 获取所有服务器
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<object>> GetAllServers()
        {
            var data = await _baseServicesServers.Dal.Db.Queryable<TrojanServers>().OrderBy(t => t.servername).Select(t => new { t.id, t.servername,t.isAllUser }).ToListAsync();
            return MessageModel<object>.Success("获取成功", data);
        }

        /// <summary>
        /// 获取Trojan服务器
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<TrojanServers>>> GetServers(string key)
        {
            var whereFind = LinqHelper.True<TrojanServers>();
            if (!string.IsNullOrEmpty(key))
                whereFind = whereFind.And(t => t.servername.Contains(key) || t.serveraddress.Contains(key) || t.serverpath.Contains(key) || t.serverpeer.Contains(key) || t.serverremark.Contains(key));
            var data = await _baseServicesServers.Dal.Query(whereFind, "servername asc");
            


            var bindUsers = await _baseServicesServersUsers.Dal.Query();
            foreach (var server in data)
            {
                server.bindUsers = bindUsers.FindAll(t => t.serverid == server.id).Select(t=>t.userid).ToList();
            }

            var excludeUsers = await _baseServicesServersUsersExclude.Dal.Query();
            foreach (var server in data)
            {
                server.excludeUsers = excludeUsers.FindAll(t => t.serverid == server.id).Select(t => t.userid).ToList();
            }


            return MessageModel<List<TrojanServers>>.Success("获取成功", data);
        }


        /// <summary>
        /// 删除Trojan服务器
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
        [HttpDelete]
        public async Task<MessageModel<string>> DelServer(long id)
        {
            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _baseServicesServers.Dal.DeleteById(id);
                await _baseServicesServersUsers.Dal.Delete(t => t.serverid == id);
                await _baseServicesServersUsersExclude.Dal.Delete(t => t.serverid == id);
                _unitOfWorkManage.CommitTran();

                if (data)
                    return MessageModel<string>.Success("删除成功");
                else
                    return MessageModel<string>.Fail("删除失败");
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
        }
        /// <summary>
        /// 删除Trojan服务器
        /// </summary>
        /// <param name="servers"></param>
        /// <returns></returns> 
        [HttpPost]
        public async Task<MessageModel<string>> DelServers([FromBody] object[] servers)
        {
            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _baseServicesServers.Dal.DeleteByIds(servers);
                await _baseServicesServersUsers.Dal.Delete(t => servers.Contains(t.serverid));
                await _baseServicesServersUsersExclude.Dal.Delete(t => servers.Contains(t.serverid));
                _unitOfWorkManage.CommitTran();

                if (data)
                    return MessageModel<string>.Success("删除成功");
                else
                    return MessageModel<string>.Fail("删除失败");
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
        }
        /// <summary>
        /// 更新Trojan服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns> 
        [HttpPut]
        public async Task<MessageModel<string>> UpdateServers(TrojanServers server)
        {
            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _baseServicesServers.Dal.Update(server);

                List<TrojanServersUsers> trojanServersUsers = new List<TrojanServersUsers>();
                server.bindUsers.ForEach(uid => trojanServersUsers.Add(new TrojanServersUsers { userid = uid, serverid = server.id }));
                await _baseServicesServersUsers.Dal.Delete(t => t.serverid == server.id);
                await _baseServicesServersUsers.Dal.Add(trojanServersUsers);

                List<TrojanServersUsersExclude> trojanServersUsersExclude = new List<TrojanServersUsersExclude>();
                server.excludeUsers.ForEach(uid => trojanServersUsersExclude.Add(new TrojanServersUsersExclude { userid = uid, serverid = server.id }));
                await _baseServicesServersUsersExclude.Dal.Delete(t => t.serverid == server.id);
                await _baseServicesServersUsersExclude.Dal.Add(trojanServersUsersExclude);


                _unitOfWorkManage.CommitTran();
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }


            return MessageModel<string>.Success("更新成功");
        }
        /// <summary>
        /// 添加Trojan服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns> 
        [HttpPost]
        public async Task<MessageModel<string>> AddServers(TrojanServers server)
        {

            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _baseServicesServers.Dal.Add(server);

                List<TrojanServersUsers> trojanServersUsers = new List<TrojanServersUsers>();
                server.bindUsers.ForEach(uid => trojanServersUsers.Add(new TrojanServersUsers { userid = uid, serverid = server.id }));
                await _baseServicesServersUsers.Dal.Delete(t => t.serverid == server.id);
                await _baseServicesServersUsers.Dal.Add(trojanServersUsers);

                List<TrojanServersUsersExclude> trojanServersUsersExclude = new List<TrojanServersUsersExclude>();
                server.excludeUsers.ForEach(uid => trojanServersUsersExclude.Add(new TrojanServersUsersExclude { userid = uid, serverid = server.id }));
                await _baseServicesServersUsersExclude.Dal.Delete(t => t.serverid == server.id);
                await _baseServicesServersUsersExclude.Dal.Add(trojanServersUsersExclude);


                _unitOfWorkManage.CommitTran();
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }

            return MessageModel<string>.Success("添加成功");
        }
        /// <summary>
        /// 获取Cus服务器
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<TrojanCusServers>>> GetCusServers(string key)
        {
            var whereFind = LinqHelper.True<TrojanCusServers>();
            if (!string.IsNullOrEmpty(key))
                whereFind = whereFind.And(t => t.servername.Contains(key) || t.serveraddress.Contains(key) || t.serverremark.Contains(key));
            var data = await _baseServicesCusServers.Dal.Query(whereFind);
            data = data.OrderBy(t => t.servername).ToList();


            List<long> sersInts = data.Select(x => x.id).ToList();
            var bindUsers = await _baseServicesCusServersUsers.Dal.Query(t => sersInts.Contains(t.serverid));
            foreach (var server in data)
            {
                server.bindUsers = bindUsers.FindAll(t => t.serverid == server.id).Select(t => t.userid).ToList();
            }


            return MessageModel<List<TrojanCusServers>>.Success("获取成功", data);
        }
        /// <summary>
        /// 删除Cus服务器
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
        [HttpDelete]
        public async Task<MessageModel<List<TrojanCusServers>>> DelCusServer(long id)
        {
            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _baseServicesCusServers.Dal.DeleteById(id);
                await _baseServicesCusServersUsers.Dal.Delete(t => t.serverid == id);
                _unitOfWorkManage.CommitTran();
                if (data)
                    return MessageModel<List<TrojanCusServers>>.Success("删除成功");
                else
                    return MessageModel<List<TrojanCusServers>>.Fail("删除失败");

            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
           
        }
        /// <summary>
        /// 删除Cus服务器
        /// </summary>
        /// <param name="servers"></param>
        /// <returns></returns> 
        [HttpPost]
        public async Task<MessageModel<List<TrojanCusServers>>> DelCusServers([FromBody] object[] servers)
        {
            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _baseServicesCusServers.Dal.DeleteByIds(servers);
                await _baseServicesCusServersUsers.Dal.Delete(t => servers.Contains(t.serverid));
                _unitOfWorkManage.CommitTran();
                if (data)
                    return MessageModel<List<TrojanCusServers>>.Success("删除成功");
                else
                    return MessageModel<List<TrojanCusServers>>.Fail("删除失败");
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
        }
        /// <summary>
        /// 更新Cus服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns> 
        [HttpPut]
        public async Task<MessageModel<List<TrojanCusServers>>> UpdateCusServers(TrojanCusServers server)
        {
            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _baseServicesCusServers.Dal.Update(server);

                List<TrojanCusServersUsers> trojanServersUsers = new List<TrojanCusServersUsers>();
                server.bindUsers.ForEach(uid => trojanServersUsers.Add(new TrojanCusServersUsers { userid = uid, serverid = server.id }));
                await _baseServicesCusServersUsers.Dal.Delete(t => t.serverid == server.id);
                await _baseServicesCusServersUsers.Dal.Add(trojanServersUsers);
                _unitOfWorkManage.CommitTran();

                return MessageModel<List<TrojanCusServers>>.Success("更新成功");
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
        }
        /// <summary>
        /// 添加Cus服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns> 
        [HttpPost]
        public async Task<MessageModel<List<TrojanCusServers>>> AddCusServers(TrojanCusServers server)
        {
            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _baseServicesCusServers.Dal.Add(server);

                List<TrojanCusServersUsers> trojanServersUsers = new List<TrojanCusServersUsers>();
                server.bindUsers.ForEach(uid => trojanServersUsers.Add(new TrojanCusServersUsers { userid = uid, serverid = server.id }));
                await _baseServicesCusServersUsers.Dal.Delete(t => t.serverid == server.id);
                await _baseServicesCusServersUsers.Dal.Add(trojanServersUsers);
                _unitOfWorkManage.CommitTran();

                return MessageModel<List<TrojanCusServers>>.Success("添加成功");
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
        }


        /// <summary>
        /// 获取Url服务器
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        public async Task<MessageModel<List<TrojanUrlServers>>> GetUrlServers(string key)
        {
            var whereFind = LinqHelper.True<TrojanUrlServers>();
            if (!string.IsNullOrEmpty(key))
                whereFind = whereFind.And(t => t.servername.Contains(key) || t.serveraddress.Contains(key) || t.serverremark.Contains(key));
            var data = await _baseServicesUrlServers.Dal.Query(whereFind);
            data = data.OrderBy(t => t.servername).ToList();

            List<long> sersInts = data.Select(x => x.id).ToList();
            var bindUsers = await _baseServicesUrlServersUsers.Dal.Query(t => sersInts.Contains(t.serverid));
            foreach (var server in data)
            {
                server.bindUsers = bindUsers.FindAll(t => t.serverid == server.id).Select(t => t.userid).ToList();
            }

            return MessageModel<List<TrojanUrlServers>>.Success("获取成功", data);
        }

        /// <summary>
        /// 删除Url服务器
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
        [HttpDelete]
        public async Task<MessageModel<List<TrojanUrlServers>>> DelUrlServer(long id)
        {
            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _baseServicesUrlServers.Dal.DeleteById(id);
                await _baseServicesUrlServersUsers.Dal.Delete(t => t.serverid == id);
                _unitOfWorkManage.CommitTran();
                if (data)
                    return MessageModel<List<TrojanUrlServers>>.Success("删除成功");
                else
                    return MessageModel<List<TrojanUrlServers>>.Fail("删除失败");
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
        }
        /// <summary>
        /// 删除Url服务器
        /// </summary>
        /// <param name="servers"></param>
        /// <returns></returns> 
        [HttpPost]
        public async Task<MessageModel<List<TrojanUrlServers>>> DelUrlServers([FromBody] object[] servers)
        {
            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _baseServicesUrlServers.Dal.DeleteByIds(servers);
                await _baseServicesUrlServersUsers.Dal.Delete(t => servers.Contains(t.serverid));
                _unitOfWorkManage.CommitTran();
                if (data)
                    return MessageModel<List<TrojanUrlServers>>.Success("删除成功");
                else
                    return MessageModel<List<TrojanUrlServers>>.Fail("删除失败");

            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
        }
        /// <summary>
        /// 更新Url服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns> 
        [HttpPut]
        public async Task<MessageModel<List<TrojanUrlServers>>> UpdateUrlServers(TrojanUrlServers server)
        {
            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _baseServicesUrlServers.Dal.Update(server);

                List<TrojanUrlServersUsers> trojanServersUsers = new List<TrojanUrlServersUsers>();
                server.bindUsers.ForEach(uid => trojanServersUsers.Add(new TrojanUrlServersUsers { userid = uid, serverid = server.id }));
                await _baseServicesUrlServersUsers.Dal.Delete(t => t.serverid == server.id);
                await _baseServicesUrlServersUsers.Dal.Add(trojanServersUsers);
                _unitOfWorkManage.CommitTran();

                return MessageModel<List<TrojanUrlServers>>.Success("更新成功");

            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
        }
        /// <summary>
        /// 添加Url服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns> 
        [HttpPost]
        public async Task<MessageModel<List<TrojanUrlServers>>> AddUrlServers(TrojanUrlServers server)
        {
            try
            {
                _unitOfWorkManage.BeginTran();
                var data = await _baseServicesUrlServers.Dal.Add(server);
                List<TrojanUrlServersUsers> trojanServersUsers = new List<TrojanUrlServersUsers>();
                server.bindUsers.ForEach(uid => trojanServersUsers.Add(new TrojanUrlServersUsers { userid = uid, serverid = server.id }));
                await _baseServicesUrlServersUsers.Dal.Delete(t => t.serverid == server.id);
                await _baseServicesUrlServersUsers.Dal.Add(trojanServersUsers);
                _unitOfWorkManage.CommitTran();

            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
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


                //默认服务器
                var bindUsers = await _baseServicesServersUsers.Dal.Db.Queryable<TrojanServersUsers>().Where(t => t.userid == user.id).Select(t => t.serverid).ToListAsync();

                var excludeUsers = await _baseServicesServersUsersExclude.Dal.Db.Queryable<TrojanServersUsersExclude>().Where(t => t.userid == user.id).Select(t => t.serverid).ToListAsync();

                var data = await _baseServicesServers.Dal.Query(t => t.serverenable && (t.isAllUser || bindUsers.Contains(t.id) ) && !excludeUsers.Contains(t.id));
                data = data.OrderBy(t => t.servername).ToList();
                if (data != null)
                {
                    data = data.OrderBy(t => t.servername).ToList();
                    foreach (var item in data)
                    {
                        sb.AppendLine(GetSplice(item, user.passwordshow));
                    }
                }
                //自定义服务器
                var bindCusUsers = await _baseServicesCusServersUsers.Dal.Db.Queryable<TrojanCusServersUsers>().Where(t => t.userid == user.id).Select(t => t.serverid).ToListAsync();
                var cusData = await _baseServicesCusServers.Dal.Query(t => t.serverenable && (t.isAllUser || bindCusUsers.Contains(t.id)));
                if (cusData != null)
                {
                    cusData = cusData.OrderBy(t => t.servername).ToList();
                    foreach (var item in cusData)
                    {
                        sb.AppendLine(item.serveraddress);
                    }
                }
                //url服务器
                var bindUrlUsers = await _baseServicesUrlServersUsers.Dal.Db.Queryable<TrojanUrlServersUsers>().Where(t => t.userid == user.id).Select(t => t.serverid).ToListAsync();
                var urlData = await _baseServicesUrlServers.Dal.Query(t => t.serverenable && (t.isAllUser || bindUrlUsers.Contains(t.id)));
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