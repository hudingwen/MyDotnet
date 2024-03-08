using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.WeChat;
using MyDotnet.Helper;
using MyDotnet.Services;

namespace MyDotnet.Controllers.WeChat
{
    /// <summary>
	/// 微信公司管理
	/// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public partial class WeChatCompanyController : Controller
    {
        public BaseServices<WeChatCompany> _baseWeChatCompanyServices;

        public WeChatCompanyController(BaseServices<WeChatCompany> baseWeChatCompanyServices)
        {
            _baseWeChatCompanyServices = baseWeChatCompanyServices;
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="pagination">分页条件</param> 
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<WeChatCompany>>> Get([FromQuery] PaginationModel pagination)
        {
            var whereFind = LinqHelper.True<WeChatCompany>();
            if (!string.IsNullOrEmpty(pagination.key))
            {
                whereFind = whereFind.And(t => t.CompanyID.Contains(pagination.key) || t.CompanyName.Contains(pagination.key));
            }
            var data = await _baseWeChatCompanyServices.Dal.QueryPage(whereFind, pagination.page,pagination.size);
            return new MessageModel<PageModel<WeChatCompany>> { success = true, response = data };
        }
        /// <summary>
        /// 获取(id)
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<MessageModel<WeChatCompany>> Get(string id)
        {
            var data = await _baseWeChatCompanyServices.Dal.QueryById(id);
            return new MessageModel<WeChatCompany> { success = true, response = data };
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] WeChatCompany obj)
        {
            await _baseWeChatCompanyServices.Dal.Db.Insertable(obj).ExecuteCommandAsync();
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] WeChatCompany obj)
        {
            await _baseWeChatCompanyServices.Dal.Update(obj);
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 删除
        /// </summary> 
        /// <returns></returns> 
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(string id)
        {
            await _baseWeChatCompanyServices.Dal.DeleteById(id);
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> BatchDelete([FromBody] object[] ids)
        {
            await _baseWeChatCompanyServices.Dal.DeleteByIds(ids);
            return new MessageModel<string> { success = true };
        }

    }
}