using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Domain.Entity.Trojan;
using MyDotnet.Helper;
using MyDotnet.Services.System;

namespace MyDotnet.Controllers.System
{
    /// <summary>
    /// 角色管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
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
        /// <param name="size"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<MessageModel<PageModel<Role>>> Get(int page = 1, int size = 10, string key = "")
        {

            var whereFind = LinqHelper.True<Role>().And(t => t.IsDeleted == false);

            if (!string.IsNullOrEmpty(key))
            {
                whereFind = whereFind.And(t => t.Name.Contains(key) || t.Description.Contains(key));
            }

            var data = await _roleServices.Dal.QueryPage(whereFind, page, size, " Id desc");

            return Success(data, "获取成功");

        }

        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        // POST: api/User
        [HttpPost]
        [Authorize(Permissions.Name)]
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
        [Authorize(Permissions.Name)]
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
        [Authorize(Permissions.Name)]
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

        [HttpPost]
        public async Task<MessageModel<string>> Deletes([FromBody] object[] ids)
        {
            var data = new MessageModel<string>();
            var models = await _roleServices.Dal.QueryByIDs(ids);
            foreach (var model in models)
            {
                model.IsDeleted = true;
            }
            data.success = await _roleServices.Dal.Update(models);
            if (data.success)
            {
                data.msg = "删除成功";
            }


            return data;
        }

    }
}
