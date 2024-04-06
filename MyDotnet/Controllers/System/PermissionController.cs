using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using MyDotnet.Services.System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace MyDotnet.Controllers.System
{
    /// <summary>
    /// 菜单管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class PermissionController : BaseApiController
    {
        public UnitOfWorkManage _unitOfWorkManage;
        public BaseServices<Permission> _permissionServices;
        public BaseServices<Modules> _moduleServices;
        public RoleModulePermissionServices _roleModulePermissionServices;
        public UserRoleServices _userRoleServices;
        public IHttpClientFactory _httpClientFactory;
        public IHttpContextAccessor _httpContext;
        public AspNetUser _user;
        public PermissionRequirement _requirement;

        public PermissionController(UnitOfWorkManage unitOfWorkManage
            , BaseServices<Permission> permissionServices
            , BaseServices<Modules> moduleServices 
            , RoleModulePermissionServices roleModulePermissionServices 
            , UserRoleServices userRoleServices 
            , IHttpClientFactory httpClientFactory 
            , IHttpContextAccessor httpContext
            , AspNetUser user 
            , PermissionRequirement requirement 
            )
        {
            _unitOfWorkManage = unitOfWorkManage;
            _permissionServices = permissionServices;
            _moduleServices = moduleServices;
            _roleModulePermissionServices = roleModulePermissionServices;
            _userRoleServices = userRoleServices;
            _httpClientFactory = httpClientFactory;
            _httpContext = httpContext;
            _user = user;
            _requirement = requirement;

        }

        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        // GET: api/User
        [HttpGet]
        public async Task<MessageModel<PageModel<Permission>>> Get(int page = 1, string key = "", int pageSize = 50)
        {
            PageModel<Permission> permissions = new PageModel<Permission>();
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }

            permissions = await _permissionServices.Dal.QueryPage(a => a.IsDeleted != true && a.Name != null && a.Name.Contains(key), page, pageSize, " Id desc ");


            #region 单独处理

            var apis = await _moduleServices.Dal.Query(d => d.IsDeleted == false);
            var permissionsView = permissions.data;

            var permissionAll = await _permissionServices.Dal.Query(d => d.IsDeleted != true);
            foreach (var item in permissionsView)
            {
                List<long> pidarr = new()
                {
                    item.Pid
                };
                if (item.Pid > 0)
                {
                    pidarr.Add(0);
                }
                var parent = permissionAll.FirstOrDefault(d => d.Id == item.Pid);

                while (parent != null)
                {
                    pidarr.Add(parent.Id);
                    parent = permissionAll.FirstOrDefault(d => d.Id == parent.Pid);
                }


                item.PidArr = pidarr.OrderBy(d => d).Distinct().ToList();
                foreach (var pid in item.PidArr)
                {
                    var per = permissionAll.FirstOrDefault(d => d.Id == pid);
                    item.PnameArr.Add((per != null ? per.Name : "根节点") + "/");
                    //var par = Permissions.Where(d => d.Pid == item.Id ).ToList();
                    //item.PCodeArr.Add((per != null ? $"/{per.Code}/{item.Code}" : ""));
                    //if (par.Count == 0 && item.Pid == 0)
                    //{
                    //    item.PCodeArr.Add($"/{item.Code}");
                    //}
                }

                item.MName = apis.FirstOrDefault(d => d.Id == item.Mid)?.LinkUrl;
            }

            permissions.data = permissionsView;

            #endregion


            //return new MessageModel<PageModel<Permission>>()
            //{
            //    msg = "获取成功",
            //    success = permissions.dataCount >= 0,
            //    response = permissions
            //};

            return permissions.dataCount >= 0 ? Success(permissions, "获取成功") : Failed<PageModel<Permission>>("获取失败");

        }

        /// <summary>
        /// 查询树形 Table
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<Permission>>> GetTreeTable()
        {

           

            List<Permission> permissions = new List<Permission>();
            var apiList = await _moduleServices.Dal.Query(d => d.IsDeleted == false);


            var permissionsList = await _permissionServices.Dal.Query(d => d.IsDeleted == false);

           

            Permission rootRoot = new Permission
            {
                Id = 0,
                Pid = 0,
                Name = "根节点"
            };

            permissionsList = permissionsList.OrderBy(d => d.OrderSort).ToList();


            RecursionHelper.LoopToAppendChildren(permissionsList, rootRoot, 0, apiList);


            return Success(rootRoot.children, "获取成功");
        }

        /// <summary>
        /// 添加一个菜单
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        // POST: api/User
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Permission permission)
        {
            //var data = new MessageModel<string>();

            permission.CreateId = _user.ID;
            permission.CreateBy = _user.Name;

            var id = await _permissionServices.Dal.Add(permission);
            //data.success = id > 0;
            //if (data.success)
            //{
            //    data.response = id.ObjToString();
            //    data.msg = "添加成功";
            //}


            return id > 0 ? Success(id.ObjToString(), "添加成功") : Failed("添加失败");
        }

        /// <summary>
        /// 保存菜单权限分配
        /// </summary>
        /// <param name="assignView"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> Assign([FromBody] AssignView assignView)
        {
            if (assignView.rid > 0)
            {
                //开启事务
                try
                {
                    var old_rmps = await _roleModulePermissionServices.Dal.Query(d => d.RoleId == assignView.rid);

                    _unitOfWorkManage.BeginTran();
                    await _permissionServices.Dal.Db.Deleteable<RoleModulePermission>(t => t.RoleId == assignView.rid).ExecuteCommandAsync();
                    var permissions = await _permissionServices.Dal.Query(d => d.IsDeleted == false);

                    List<RoleModulePermission> new_rmps = new List<RoleModulePermission>();
                    var nowTime = _permissionServices.Dal.Db.GetDate();
                    foreach (var item in assignView.pids)
                    {
                        var moduleid = permissions.Find(p => p.Id == item)?.Mid;
                        var find_old_rmps = old_rmps.Find(p => p.PermissionId == item);

                        RoleModulePermission roleModulePermission = new RoleModulePermission()
                        {
                            Enabled = true,
                            RoleId = assignView.rid,
                            ModuleId = moduleid.Value,
                            PermissionId = item
                        };
                        new_rmps.Add(roleModulePermission);
                    }
                    if (new_rmps.Count > 0) await _roleModulePermissionServices.Dal.Add(new_rmps);
                    _unitOfWorkManage.CommitTran();
                }
                catch (Exception)
                {
                    _unitOfWorkManage.RollbackTran();
                    throw;
                }
                _requirement.Permissions.Clear();
                return Success<string>("保存成功");
            }
            else
            {
                return Failed<string>("请选择要操作的角色");
            }
        }


        /// <summary>
        /// 获取菜单树
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="needbtn"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PermissionTree>> GetPermissionTree(long pid = 0, bool needbtn = false)
        {
            //var data = new MessageModel<PermissionTree>();

            var permissions = await _permissionServices.Dal.Query(d => d.IsDeleted == false);
            var permissionTrees = (from child in permissions
                                   where child.IsDeleted == false
                                   orderby child.Id
                                   select new PermissionTree
                                   {
                                       value = child.Id,
                                       label = child.Name,
                                       Pid = child.Pid,
                                       isbtn = child.IsButton,
                                       order = child.OrderSort,
                                   }).ToList();
            PermissionTree rootRoot = new PermissionTree
            {
                value = 0,
                Pid = 0,
                label = "根节点"
            };

            permissionTrees = permissionTrees.OrderBy(d => d.order).ToList();


            RecursionHelper.LoopToAppendChildren(permissionTrees, rootRoot, pid, needbtn);

            //data.success = true;
            //if (data.success)
            //{
            //    data.response = rootRoot;
            //    data.msg = "获取成功";
            //}

            return Success(rootRoot, "获取成功");
            //return data;
        }

        /// <summary>
        /// 获取路由树
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<NavigationBar>> GetNavigationBar(long uid)
        {

            var data = new MessageModel<NavigationBar>();

            long uidInHttpcontext1 = 0;
            var roleIds = new List<long>();

            uidInHttpcontext1 = (JWTHelper.SerializeJwtStr(_httpContext.HttpContext.Request.Headers["Authorization"].ObjToString().Replace("Bearer ", ""))?.Uid).ObjToLong();
            roleIds = (await _userRoleServices.Dal.Query(d => d.IsDeleted == false && d.UserId == uid)).Select(d => d.RoleId.ObjToLong()).Distinct().ToList();


            if (uid > 0 && uid == uidInHttpcontext1)
            {

                NavigationBar rootRoot = new NavigationBar()
                {
                    id = 0,
                    pid = 0,
                    order = 0,
                    name = "根节点",
                    path = "",
                    iconCls = "",
                    meta = new NavigationBarMeta(),
                    children = new List<NavigationBar>()
                };
                if (roleIds.Any())
                {
                    var pids = (await _roleModulePermissionServices.Dal.Query(d => d.IsDeleted == false && roleIds.Contains(d.RoleId))).Select(d => d.PermissionId.ObjToLong()).Distinct();
                    if (pids.Any())
                    {
                        var rolePermissionMoudles = (await _permissionServices.Dal.Query(d => pids.Contains(d.Id))).OrderBy(c => c.OrderSort);

                        var permissionTrees = (from child in rolePermissionMoudles
                                               where child.IsDeleted == false
                                               orderby child.Id
                                               select new NavigationBar
                                               {
                                                   id = child.Id,
                                                   name = child.Name,
                                                   pid = child.Pid,
                                                   order = child.OrderSort,
                                                   path = child.Code,
                                                   iconCls = child.Icon,
                                                   Func = child.Func,
                                                   IsHide = child.IsHide.ObjToBool(),
                                                   IsButton = child.IsButton.ObjToBool(),
                                                   meta = new NavigationBarMeta
                                                   {
                                                       requireAuth = true,
                                                       title = child.Name,
                                                       NoTabPage = child.IsHide.ObjToBool(),
                                                       keepAlive = child.IskeepAlive.ObjToBool()
                                                   }
                                               }).ToList();



                        permissionTrees = permissionTrees.OrderBy(d => d.order).ToList();
                        RecursionHelper.LoopNaviBarAppendChildren(permissionTrees, rootRoot);

                    }
                }


                data.success = true;
                data.response = rootRoot;
                data.msg = "获取成功";
            }
            return data;
        }

        /// <summary>
        /// 获取路由树
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<NavigationBarPro>>> GetNavigationBarPro(long uid)
        {
            var data = new MessageModel<List<NavigationBarPro>>();

            long uidInHttpcontext1 = 0;
            var roleIds = new List<long>();

            // jwt
            uidInHttpcontext1 = (JWTHelper.SerializeJwtStr(_httpContext.HttpContext.Request.Headers["Authorization"].ObjToString().Replace("Bearer ", ""))?.Uid).ObjToLong();
            roleIds = (await _userRoleServices.Dal.Query(d => d.IsDeleted == false && d.UserId == uid)).Select(d => d.RoleId.ObjToLong()).Distinct().ToList();

            if (uid > 0 && uid == uidInHttpcontext1)
            {
                if (roleIds.Any())
                {
                    var pids = (await _roleModulePermissionServices.Dal.Query(d => d.IsDeleted == false && roleIds.Contains(d.RoleId)))
                                    .Select(d => d.PermissionId.ObjToLong()).Distinct();
                    if (pids.Any())
                    {
                        var rolePermissionMoudles = (await _permissionServices.Dal.Query(d => pids.Contains(d.Id) && d.IsButton == false)).OrderBy(c => c.OrderSort);
                        var permissionTrees = (from item in rolePermissionMoudles
                                               where item.IsDeleted == false
                                               orderby item.Id
                                               select new NavigationBarPro
                                               {
                                                   id = item.Id,
                                                   name = item.Name,
                                                   parentId = item.Pid,
                                                   order = item.OrderSort,
                                                   path = item.Code == "-" ? "-" : item.Code == "/" ? "/dashboard/workplace" : item.Code,
                                                   component = item.Pid == 0 ? item.Code == "/" ? "dashboard/Workplace" : "RouteView" : item.Code?.TrimStart('/'),
                                                   iconCls = item.Icon,
                                                   Func = item.Func,
                                                   IsHide = item.IsHide.ObjToBool(),
                                                   IsButton = item.IsButton.ObjToBool(),
                                                   meta = new NavigationBarMetaPro
                                                   {
                                                       show = true,
                                                       title = item.Name,
                                                       icon = "user"//item.Icon
                                                   }
                                               }).ToList();

                        permissionTrees = permissionTrees.OrderBy(d => d.order).ToList();


                        data.success = true;
                        data.response = permissionTrees;
                        data.msg = "获取成功";
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// 通过角色获取菜单
        /// </summary>
        /// <param name="rid"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<AssignShow>> GetPermissionIdByRoleId(long rid = 0)
        {
            //var data = new MessageModel<AssignShow>();

            var rmps = await _roleModulePermissionServices.Dal.Query(d => d.IsDeleted == false && d.RoleId == rid);
            var permissionTrees = (from child in rmps
                                   orderby child.Id
                                   select child.PermissionId.ObjToLong()).ToList();

            var permissions = await _permissionServices.Dal.Query(d => d.IsDeleted == false);
            List<string> assignbtns = new List<string>();

            foreach (var item in permissionTrees)
            {
                var pername = permissions.FirstOrDefault(d => d.IsButton && d.Id == item)?.Name;
                if (!string.IsNullOrEmpty(pername))
                {
                    //assignbtns.Add(pername + "_" + item);
                    assignbtns.Add(item.ObjToString());
                }
            }

            //data.success = true;
            //if (data.success)
            //{
            //    data.response = new AssignShow()
            //    {
            //        permissionids = permissionTrees,
            //        assignbtns = assignbtns,
            //    };
            //    data.msg = "获取成功";
            //}

            return Success(new AssignShow()
            {
                permissionids = permissionTrees,
                assignbtns = assignbtns,
            }, "获取成功");

            //return data;
        }

        /// <summary>
        /// 更新菜单
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        // PUT: api/User/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Permission permission)
        {
            var data = new MessageModel<string>();
            if (permission != null && permission.Id > 0)
            {
                data.success = await _permissionServices.Dal.Update(permission);
                await _roleModulePermissionServices.UpdateModuleId(permission.Id, permission.Mid);
                if (data.success)
                {
                    data.msg = "更新成功";
                    data.response = permission?.Id.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/ApiWithActions/5
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(long id)
        {
            var data = new MessageModel<string>();
            if (id > 0)
            {
                var userDetail = await _permissionServices.Dal.QueryById(id);
                userDetail.IsDeleted = true;
                data.success = await _permissionServices.Dal.Update(userDetail);
                if (data.success)
                {
                    data.msg = "删除成功";
                    data.response = userDetail?.Id.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        /// 导入多条菜单信息
        /// </summary>
        /// <param name="permissions"></param>
        /// <returns></returns>
        // POST: api/User
        [HttpPost]
        public async Task<MessageModel<string>> BatchPost([FromBody] List<Permission> permissions)
        {
            var data = new MessageModel<string>();
            string ids = string.Empty;
            int sucCount = 0;

            for (int i = 0; i < permissions.Count; i++)
            {
                var permission = permissions[i];
                if (permission != null)
                {
                    permission.CreateId = _user.ID;
                    permission.CreateBy = _user.Name;
                    ids += await _permissionServices.Dal.Add(permission);
                    sucCount++;
                }
            }

            data.success = ids.IsNotEmptyOrNull();
            if (data.success)
            {
                data.response = ids;
                data.msg = $"{sucCount}条数据添加成功";
            }

            return data;
        }
    }

    public class AssignView
    {
        public List<long> pids { get; set; }
        public long rid { get; set; }
    }
    public class AssignShow
    {
        public List<long> permissionids { get; set; }
        public List<string> assignbtns { get; set; }
    }

}
