using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using MyDotnet.Services.System;

namespace MyDotnet.Controllers.System
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : BaseApiController
    {
        public UnitOfWorkManage _unitOfWorkManage;
        public SysUserInfoServices _sysUserInfoServices;
        public BaseServices<UserGoogleAuthenticator> _userGoogleAuthService;
        public UserRoleServices _userRoleServices;
        public RoleServices _roleServices;
        public BaseServices<Department> _departmentServices;
        public AspNetUser _user;
        public IMapper _mapper;
        public IHttpContextAccessor _httpContext;
        public DicService _dictService;

        public UserController(UnitOfWorkManage unitOfWorkManage
            , SysUserInfoServices sysUserInfoServices
            , UserRoleServices userRoleServices
            , RoleServices roleServices
            , BaseServices<Department> departmentServices
            , BaseServices<UserGoogleAuthenticator> userGoogleAuthService
            , AspNetUser user
            , IMapper mapper
            , IHttpContextAccessor httpContext
            , DicService dictService
            )
        {
            _unitOfWorkManage = unitOfWorkManage;
            _sysUserInfoServices = sysUserInfoServices;
            _userRoleServices = userRoleServices;
            _roleServices = roleServices;
            _departmentServices = departmentServices;
            _user = user;
            _mapper = mapper;
            _httpContext = httpContext;
            _userGoogleAuthService = userGoogleAuthService;
            _dictService = dictService;

        }
        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]

        [Authorize(Permissions.Name)]
        public async Task<MessageModel<PageModel<SysUserInfo>>> Get(int page = 1, int size = 10, string key = "")
        {
            var whereFind = LinqHelper.True<SysUserInfo>().And(t => t.IsDeleted == false);

            if (!string.IsNullOrEmpty(key))
            {
                whereFind = whereFind.And(t => t.Name.Contains(key) || t.LoginName.Contains(key) || t.RealName.Contains(key));
            }

            var data = await _sysUserInfoServices.Dal.QueryPage(whereFind, page, size, " Id desc ");




            // 这里可以封装到多表查询，此处简单处理
            var allUserRoles = await _userRoleServices.Dal.Query(d => d.IsDeleted == false);
            var allRoles = await _roleServices.Dal.Query(d => d.IsDeleted == false);
            var allDepartments = await _departmentServices.Dal.Query(d => d.IsDeleted == false);

            var sysUserInfos = data.data;
            foreach (var item in sysUserInfos)
            {
                var currentUserRoles = allUserRoles.Where(d => d.UserId == item.Id).Select(d => d.RoleId).ToList();
                item.RIDs = currentUserRoles;
                item.RoleNames = allRoles.Where(d => currentUserRoles.Contains(d.Id)).Select(d => d.Name).ToList();
                var departmentNameAndIds = GetFullDepartmentName(allDepartments, item.DepartmentId);
                item.DepartmentName = departmentNameAndIds.Item1;
                item.Dids = departmentNameAndIds.Item2;
            }

            data.data = sysUserInfos;



            var oldData =  data.ConvertTo<SysUserInfoDto>(_mapper);
            return Success(data);
        }

        private (string, List<long>) GetFullDepartmentName(List<Department> departments, long departmentId)
        {
            var departmentModel = departments.FirstOrDefault(d => d.Id == departmentId);
            if (departmentModel == null)
            {
                return ("", new List<long>());
            }

            var pids = departmentModel.CodeRelationship?.TrimEnd(',').Split(',').Select(d => d.ObjToLong()).ToList();
            pids.Add(departmentModel.Id);
            var pnams = departments.Where(d => pids.Contains(d.Id)).ToList().Select(d => d.Name).ToArray();
            var fullName = string.Join("/", pnams);

            return (fullName, pids);
        }


        // GET: api/User/5
        /// <summary>
        /// 获取用户详情根据token
        /// 【无权限】
        /// </summary>
        /// <param name="token">令牌</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<SysUserInfo>> GetInfoByToken(string token)
        {
            var data = new MessageModel<SysUserInfo>();
            if (!string.IsNullOrEmpty(token))
            {
                var tokenModel = JWTHelper.SerializeJwtStr(token);
                if (tokenModel != null && tokenModel.Uid > 0)
                {
                    var userinfo = await _sysUserInfoServices.Dal.QueryById(tokenModel.Uid);
                    if (userinfo != null)
                    {

                        var allUserRoles = await _userRoleServices.Dal.Query(d => d.IsDeleted == false);
                        var currentUserRoles = allUserRoles.Where(d => d.UserId == userinfo.Id).Select(d => d.RoleId).ToList();
                        userinfo.RIDs = currentUserRoles;

                        data.response = userinfo;
                        data.success = true;
                        data.msg = "获取成功";
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// 添加一个用户
        /// </summary>
        /// <param name="sysUserInfo"></param>
        /// <returns></returns>
        // POST: api/User
        [HttpPost]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> Post([FromBody] SysUserInfo sysUserInfo)
        {
            var data = new MessageModel<string>();

            sysUserInfo.LoginPWD = MD5Helper.MD5Encrypt32(sysUserInfo.LoginPWD);

            //登录账号判断
            var hasUser = await _sysUserInfoServices.Dal.Query(t => t.LoginName == sysUserInfo.LoginName && t.IsDeleted == false);
            if (hasUser.Count > 0)
            {
                return Failed<string>($"登录名:{sysUserInfo.LoginName}已存在,请重新填写!");
            }
            try
            {
                _unitOfWorkManage.BeginTran();

                var id = await _sysUserInfoServices.Dal.Add(sysUserInfo);


                // 添加角色
                if (sysUserInfo.RIDs != null && sysUserInfo.RIDs.Count > 0)
                {
                    var userRolsAdd = new List<UserRole>();
                    sysUserInfo.RIDs.ForEach(rid => { userRolsAdd.Add(new UserRole(id, rid)); });
                    await _userRoleServices.Dal.Add(userRolsAdd);
                }
                _unitOfWorkManage.CommitTran();
                data.success = id > 0;
                if (data.success)
                {
                    data.response = id.ObjToString();
                    data.msg = "添加成功";
                }
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }

            

            return data;
        }
        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="sysUserInfo"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> ResetPass([FromBody] SysUserInfo sysUserInfo)
        {
            //重置密码

            var oldUser = await _sysUserInfoServices.Dal.QueryById(sysUserInfo.Id);
            if (oldUser == null || oldUser.IsDeleted)
            {
                return Failed<string>("用户不存在或已被删除");
            }
            sysUserInfo.LoginPWD = MD5Helper.MD5Encrypt32(sysUserInfo.LoginPWD);
            await _sysUserInfoServices.Dal.Update(sysUserInfo, t => new { t.LoginPWD });
            return Success<string>("重置成功");
        }

        /// <summary>
        /// 更新我的头像
        /// </summary>
        /// <param name="sysUserInfo"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public async Task<MessageModel<string>> RefreshMyLogo([FromBody] SysUserInfo sysUserInfo)
        {
            //重置密码 登录既可修改
            var uid = (JWTHelper.SerializeJwtStr(_httpContext.HttpContext.Request.Headers["Authorization"].ObjToString().Replace("Bearer ", ""))?.Uid).ObjToLong();
            var oldUser = await _sysUserInfoServices.Dal.QueryById(uid);
            if (oldUser == null || oldUser.IsDeleted)
            {
                return Failed<string>("用户不存在或已被删除");
            }
            sysUserInfo.Id = uid;
            await _sysUserInfoServices.Dal.Update(sysUserInfo, t => new { t.logo });
            return Success<string>("更新成功");
        }

        /// <summary>
        /// 重置我的密码
        /// </summary>
        /// <param name="sysUserInfo"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public async Task<MessageModel<string>> ResetMyPass([FromBody] SysUserInfo sysUserInfo)
        {
            //重置密码 登录既可修改
            var uid = (JWTHelper.SerializeJwtStr(_httpContext.HttpContext.Request.Headers["Authorization"].ObjToString().Replace("Bearer ", ""))?.Uid).ObjToLong();
            var oldUser = await _sysUserInfoServices.Dal.QueryById(uid);
            if (oldUser == null || oldUser.IsDeleted)
            {
                return Failed<string>("用户不存在或已被删除");
            }
            sysUserInfo.Id = uid;
            sysUserInfo.LoginPWD = MD5Helper.MD5Encrypt32(sysUserInfo.LoginPWD);
            await _sysUserInfoServices.Dal.Update(sysUserInfo, t => new { t.LoginPWD });
            return Success<string>("重置成功");
        }
        /// <summary>
        /// 更新我的资料
        /// </summary>
        /// <param name="sysUserInfo"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public async Task<MessageModel<string>> PutMyInfo([FromBody] SysUserInfo sysUserInfo)
        {
            var uid = (JWTHelper.SerializeJwtStr(_httpContext.HttpContext.Request.Headers["Authorization"].ObjToString().Replace("Bearer ", ""))?.Uid).ObjToLong();
           
            var data = new MessageModel<string>();

            //登录账号判断
            var hasUser = await _sysUserInfoServices.Dal.Query(t => t.LoginName == sysUserInfo.LoginName && t.IsDeleted == false && t.Id != uid);
            if (hasUser.Count > 0)
            {
                return Failed<string>($"登录名:{sysUserInfo.LoginName}已存在,请重新填写!");
            }

            var oldUser = await _sysUserInfoServices.Dal.QueryById(uid);
            if (oldUser == null || oldUser.IsDeleted)
            {
                return Failed<string>("用户不存在或已被删除");
            }
            sysUserInfo.Id = uid;
            data.success = await _sysUserInfoServices.Dal.Update(sysUserInfo,t=>new {t.RealName,t.LoginName,t.Sex,t.Age,t.Birth,t.Address,t.Remark});
            if(data.success)
            {
                data.msg = "更新成功";
            }
            else
            {
                data.msg = "更新失败";
            }
            return data;
        }


        /// <summary>
        /// 获取一个新的的双因子认证
        /// </summary>
        /// <param name="sysUserInfo"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<MessageModel<UserGoogleAuthenticator>> GetMy2FA()
        {
            //获取我的双因子
            var uid = (JWTHelper.SerializeJwtStr(_httpContext.HttpContext.Request.Headers["Authorization"].ObjToString().Replace("Bearer ", ""))?.Uid).ObjToLong();
           
           
            var oldUser = await _sysUserInfoServices.Dal.QueryById(uid);
            if (oldUser == null || oldUser.IsDeleted)
            {
                return Failed(default(UserGoogleAuthenticator), "用户不存在或已被删除");
            }


            //生成新的
            var  dicIss = await  _dictService.GetDicDataOne(SysAuthInfo.KEY, SysAuthInfo.auth_issuer);
            var auth = new UserGoogleAuthenticator();
            auth.user = oldUser.LoginName;
            auth.issuer = dicIss.content;
            auth.key = StringHelper.GetGUID();
            auth.userId = uid;
            auth.Enabled = true;
            var setCode = GoogleAuthenticatorHelper.GenerateSetupCode(auth.issuer, auth.user, auth.key);
            auth.provisionUrl = setCode.provisionUrl;
            auth.keyBase32 = setCode.encodedSecretKey;
            await _userGoogleAuthService.Dal.Add(auth);
            return Success(auth);
        }
        /// <summary>
        /// 认证双因子
        /// </summary>
        /// <param name="authId"></param>
        /// <param name="authCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<MessageModel<string>> ValidMy2FA(long authId,string authCode)
        {
            //获取我的双因子
            var uid = (JWTHelper.SerializeJwtStr(_httpContext.HttpContext.Request.Headers["Authorization"].ObjToString().Replace("Bearer ", ""))?.Uid).ObjToLong();

            var oldUser = await _sysUserInfoServices.Dal.QueryById(uid);
            if (oldUser == null || oldUser.IsDeleted)
            {
                return Failed("用户不存在或已被删除");
            }

            var auth = await _userGoogleAuthService.Dal.QueryById(authId);
            if(auth == null || auth.Enabled == false || !auth.userId.Equals(uid) ) return Failed("认证失败");
            if(DateTime.Now.AddMinutes(-5) >= auth.CreateTime.Value) return Failed("二维码失效,请重新生成");

            if (GoogleAuthenticatorHelper.ValidateTwoFactorPIN(auth.key, authCode))
            {
                //认证成功
                _unitOfWorkManage.BeginTran();
                try
                {
                    oldUser.auth2faEnable = true;
                    oldUser.auth2faId = auth.Id;
                    await _sysUserInfoServices.Dal.Update(oldUser, t => new { t.auth2faEnable, t.auth2faId });

                    auth.Enabled = false;
                    await _userGoogleAuthService.Dal.Update(auth, t => new { t.Enabled });
                    _unitOfWorkManage.CommitTran();
                    return Success("认证成功");
                }
                catch (Exception ex)
                {
                    _unitOfWorkManage.RollbackTran();
                    LogHelper.logApp.Error("认证失败", ex);
                    return Failed($"认证失败:{ex.Message}");
                }
            }
            else
            {
                //认证失败
                return Failed("认证失败");
            }

        }

        /// <summary>
        /// 认证双因子
        /// </summary>
        /// <param name="authId"></param>
        /// <param name="authCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<MessageModel<string>> CancelMy2FA(string authCode)
        {
            //获取我的双因子
            var uid = (JWTHelper.SerializeJwtStr(_httpContext.HttpContext.Request.Headers["Authorization"].ObjToString().Replace("Bearer ", ""))?.Uid).ObjToLong();

            var oldUser = await _sysUserInfoServices.Dal.QueryById(uid);
            if (oldUser == null || oldUser.IsDeleted)
            {
                return Failed("用户不存在或已被删除");
            }
            if (oldUser.auth2faEnable == false)
            {
                return Failed("未开启验证");
            }


            var auth = await _userGoogleAuthService.Dal.QueryById(oldUser.auth2faId);
            if (auth == null) return Failed("认证失败");

            if (GoogleAuthenticatorHelper.ValidateTwoFactorPIN(auth.key, authCode))
            {
                //认证成功 
                oldUser.auth2faEnable = false;
                await _sysUserInfoServices.Dal.Update(oldUser, t => new { t.auth2faEnable });
                return Success("取消成功");
            }
            else
            {
                //认证失败
                return Failed("认证失败");
            }

        }

        /// <summary>
        /// 获取我的信息
        /// </summary>
        /// <param name="sysUserInfo"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<MessageModel<SysUserInfo>> GetMyInfo()
        {
            var token =_httpContext.HttpContext.Request.Headers["Authorization"].ObjToString().Replace("Bearer ", "");
            return await GetInfoByToken(token);

        }





        /// <summary>
        /// 更新用户与角色
        /// </summary>
        /// <param name="sysUserInfo"></param>
        /// <returns></returns>
        // PUT: api/User/5
        [HttpPut]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> Put([FromBody] SysUserInfo sysUserInfo)
        {
            // 这里使用事务处理
            var data = new MessageModel<string>();

            //登录账号判断
            var hasUser = await _sysUserInfoServices.Dal.Query(t => t.LoginName == sysUserInfo.LoginName && t.IsDeleted == false && t.Id != sysUserInfo.Id);
            if (hasUser.Count > 0)
            {
                return Failed<string>($"登录名:{sysUserInfo.LoginName}已存在,请重新填写!");
            }


            var oldUser = await _sysUserInfoServices.Dal.QueryById(sysUserInfo.Id);
            if (oldUser == null || oldUser.IsDeleted)
            {
                return Failed<string>("用户不存在或已被删除");
            }

            try
            {
                //if (sysUserInfo.LoginPWD != oldUser.LoginPWD && !string.IsNullOrEmpty(sysUserInfo.LoginPWD))
                //{
                //    //修改密码
                //    oldUser.CriticalModifyTime = DateTime.Now;
                //    sysUserInfo.LoginPWD = MD5Helper.MD5Encrypt32(sysUserInfo.LoginPWD);
                //}

                //_mapper.Map(sysUserInfo, oldUser);

                _unitOfWorkManage.BeginTran();
                // 无论 Update Or Add , 先删除当前用户的全部 U_R 关系
                var usreroles = await _userRoleServices.Dal.Query(d => d.UserId == oldUser.Id);
                if (usreroles.Any())
                {
                    var ids = usreroles.Select(d => d.Id.ToString()).ToArray();
                    var isAllDeleted = await _userRoleServices.Dal.DeleteByIds(ids);
                    if (!isAllDeleted)
                    {
                        return Failed("服务器更新异常");
                    }
                }

                // 然后再执行添加操作
                if (sysUserInfo.RIDs.Count > 0)
                {
                    var userRolsAdd = new List<UserRole>();
                    sysUserInfo.RIDs.ForEach(rid => { userRolsAdd.Add(new UserRole(sysUserInfo.Id, rid)); });

                    var oldRole = usreroles.Select(s => s.RoleId).OrderBy(i => i).ToArray();
                    var newRole = userRolsAdd.Select(s => s.RoleId).OrderBy(i => i).ToArray();
                    if (!oldRole.SequenceEqual(newRole))
                    {
                        sysUserInfo.CriticalModifyTime = DateTime.Now;
                    }

                    await _userRoleServices.Dal.Add(userRolsAdd);
                }

                data.success = await _sysUserInfoServices.Dal.Update(sysUserInfo);

                _unitOfWorkManage.CommitTran();

                if (data.success)
                {
                    data.msg = "更新成功";
                    data.response = sysUserInfo.Id.ObjToString();
                }
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
            return data;
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/ApiWithActions/5
        [HttpDelete]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> Delete(long id)
        {
            var data = new MessageModel<string>();
            if (id > 0)
            {
                var userDetail = await _sysUserInfoServices.Dal.QueryById(id);
                userDetail.IsDeleted = true;
                data.success = await _sysUserInfoServices.Dal.Update(userDetail);
                if (data.success)
                {
                    data.msg = "删除成功";
                    data.response = userDetail?.Id.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> Deletes([FromBody] object[] ids)
        {
            var data = new MessageModel<string>();
            var models = await _sysUserInfoServices.Dal.QueryByIDs(ids);
            foreach (var model in models)
            {
                model.IsDeleted = true;
            }
            data.success = await _sysUserInfoServices.Dal.Update(models);
            if (data.success)
            {
                data.msg = "删除成功";
            }
            return data;
        }
    }
}