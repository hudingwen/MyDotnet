using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services.System;

namespace MyDotnet.Controllers.System
{
    /// <summary>
    /// 角色管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class RoleController : BaseApiController
    {
        public RoleServices _roleServices;
        public AspNetUser _user;
        public RoleController(RoleServices roleServices
            , AspNetUser user)
        {
            _roleServices = roleServices;
            _user = user;
        }

        /// <summary>
        /// 获取全部角色
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        // GET: api/User
        [HttpGet]
        public async Task<MessageModel<PageModel<Role>>> Get(int page = 1, string key = "")
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }

            int intPageSize = 50;

            var data = await _roleServices.Dal.QueryPage(a => a.IsDeleted != true && a.Name != null && a.Name.Contains(key), page, intPageSize, " Id desc ");

            //return new MessageModel<PageModel<Role>>()
            //{
            //    msg = "获取成功",
            //    success = data.dataCount >= 0,
            //    response = data
            //};

            return Success(data, "获取成功");

        }

        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        // POST: api/User
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Role role)
        {
            role.CreateId = _user.ID;
            role.CreateBy = _user.Name;
            var id = await _roleServices.Dal.Add(role);
            return id > 0 ? Success(id.ObjToString(), "添加成功") : Failed("添加失败");

        }

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        // PUT: api/User/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Role role)
        {
            if (role == null || role.Id <= 0)
                return Failed("缺少参数");

            return await _roleServices.Dal.Update(role) ? Success(role?.Id.ObjToString(), "更新成功") : Failed("更新失败");

            //var data = new MessageModel<string>();
            //if (role != null && role.Id > 0)
            //{
            //    data.success = await _roleServices.Update(role);
            //    if (data.success)
            //    {
            //        data.msg = "更新成功";
            //        data.response = role?.Id.ObjToString();
            //    }
            //}
            //return data;
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/ApiWithActions/5
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(long id)
        {

            var data = new MessageModel<string>();
            //if (id > 0)
            //{
            //    var userDetail = await _roleServices.QueryById(id);
            //    userDetail.IsDeleted = true;
            //    data.success = await _roleServices.Update(userDetail);
            //    if (data.success)
            //    {
            //        data.msg = "删除成功";
            //        data.response = userDetail?.Id.ObjToString();
            //    }
            //}
            //return data;

            if (id <= 0) return Failed();
            var userDetail = await _roleServices.Dal.QueryById(id);
            if (userDetail == null) return Success<string>(null, "角色不存在");
            userDetail.IsDeleted = true;
            return await _roleServices.Dal.Update(userDetail) ? Success(userDetail?.Id.ObjToString(), "删除成功") : Failed();
        }
    }
}
