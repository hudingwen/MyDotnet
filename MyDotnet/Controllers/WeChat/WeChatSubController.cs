
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.WeChat;
using MyDotnet.Services;

namespace MyDotnet.Controllers.WeChat
{
    /// <summary>
	/// 微信公众号绑定用户管理
	/// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public partial class WeChatSubController : Controller
    {
        public BaseServices<WeChatSub> baseServicesWeChatSub { get; set; }
        public WeChatSubController(BaseServices<WeChatSub> _baseServicesWeChatSub)
        {
            baseServicesWeChatSub = _baseServicesWeChatSub;
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="pagination">分页条件</param> 
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<WeChatSub>>> Get([FromQuery] PaginationModel pagination)
        {
            var data = await baseServicesWeChatSub.Dal.QueryPage(pagination);
            return new MessageModel<PageModel<WeChatSub>> { success = true, response = data };
        }
        /// <summary>
        /// 获取(id)
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<MessageModel<WeChatSub>> Get(string id)
        {
            var data = await baseServicesWeChatSub.Dal.QueryById(id);
            return new MessageModel<WeChatSub> { success = true, response = data };
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] WeChatSub obj)
        {
            await baseServicesWeChatSub.Dal.Add(obj);
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] WeChatSub obj)
        {
            await baseServicesWeChatSub.Dal.Update(obj);
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 删除
        /// </summary> 
        /// <returns></returns> 
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(string id)
        {
            await baseServicesWeChatSub.Dal.DeleteById(id);
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<MessageModel<string>> BatchDelete(string ids)
        {
            var i = ids.Split(",");
            await baseServicesWeChatSub.Dal.DeleteByIds(i);
            return new MessageModel<string> { success = true };
        }

    }
}