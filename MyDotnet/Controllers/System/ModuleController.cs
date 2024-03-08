using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.System;

namespace MyDotnet.Controllers.System
{
    /// <summary>
    /// 接口管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class ModuleController : BaseApiController
    {
        public BaseServices<Modules> _moduleServices;
        public AspNetUser _user;
        public ModuleController(BaseServices<Modules> moduleServices, AspNetUser user)
        {
            _moduleServices = moduleServices;
            _user = user;
        }
        /// <summary>
        /// 获取api接口
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<Modules>>> Get(int page = 1,int size=10 ,string key = "")
        {
            var whereFind = LinqHelper.True<Modules>().And(t => t.IsDeleted == false);

            if (!string.IsNullOrEmpty(key))
            {
                whereFind = whereFind.And(t => t.Name.Contains(key) || t.LinkUrl.Contains(key));
            }

            var data = await _moduleServices.Dal.QueryPage(whereFind, page, size, " Id desc ");

            return Success(data, "获取成功");


        }

        /// <summary>
        /// 添加一条接口信息
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        // POST: api/User
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Modules module)
        {
            module.CreateId = _user.ID;
            module.CreateBy = _user.Name;
            module.IsDeleted = false;
            var id = await _moduleServices.Dal.Add(module);
            return id > 0 ? Success(id.ObjToString(), "添加成功") : Failed();

        }

        /// <summary>
        /// 更新接口信息
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        // PUT: api/User/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Modules module)
        {
            if (module == null || module.Id <= 0)
                return Failed("缺少参数");
            return await _moduleServices.Dal.Update(module) ? Success(module?.Id.ObjToString(), "更新成功") : Failed();
        }

        /// <summary>
        /// 删除一条接口
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/ApiWithActions/5
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(long id)
        {
            if (id <= 0)
                return Failed("缺少参数");
            var userDetail = await _moduleServices.Dal.QueryById(id);
            if (userDetail == null)
                return Failed("信息不存在");

            userDetail.IsDeleted = true;
            return await _moduleServices.Dal.Update(userDetail) ? Success(userDetail?.Id.ObjToString(), "删除成功") : Failed("删除失败");
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<MessageModel<string>> Deletes([FromBody] object[] ids)
        {
            var data = new MessageModel<string>();
            var models = await _moduleServices.Dal.QueryByIDs(ids);
            foreach (var model in models)
            {
                model.IsDeleted = true;
            }
            data.success = await _moduleServices.Dal.Update(models);
            if (data.success)
            {
                data.msg = "删除成功";
            }


            return data;
        }

        /// <summary>
        /// 导入多条接口信息
        /// </summary>
        /// <param name="modules"></param>
        /// <returns></returns>
        // POST: api/User
        [HttpPost]
        public async Task<MessageModel<string>> BatchPost([FromBody] List<Modules> modules)
        {
            string ids = string.Empty;
            int sucCount = 0;

            for (int i = 0; i < modules.Count; i++)
            {
                var module = modules[i];
                if (module != null)
                {
                    module.CreateId = _user.ID;
                    module.CreateBy = _user.Name;
                    ids += await _moduleServices.Dal.Add(module);
                    sucCount++;
                }
            }
            return ids.IsNotEmptyOrNull() ? Success(ids, $"{sucCount}条数据添加成功") : Failed();
        }
    }
}
