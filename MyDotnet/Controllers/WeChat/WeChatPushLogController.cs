
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.WeChat;
using MyDotnet.Services;

namespace MyDotnet.Controllers.WeChat
{
    /// <summary>
	/// 微信推送日志管理
	/// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public partial class WeChatPushLogController : Controller
    {
        public BaseServices<WeChatPushLog> _baseServicesWeChatPushLog;
        public WeChatPushLogController(BaseServices<WeChatPushLog> baseServicesWeChatPushLog)
        {
            _baseServicesWeChatPushLog = baseServicesWeChatPushLog;
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="pagination">分页条件</param> 
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<WeChatPushLog>>> Get([FromQuery] PaginationModel pagination)
        {
            pagination.orderByFileds = "Id desc";
            var data = await _baseServicesWeChatPushLog.Dal.QueryPage(pagination);
            return new MessageModel<PageModel<WeChatPushLog>> { success = true, response = data };
        }
        /// <summary>
        /// 获取(id)
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<MessageModel<WeChatPushLog>> Get(string id)
        {
            var data = await _baseServicesWeChatPushLog.Dal.QueryById(id);
            return new MessageModel<WeChatPushLog> { success = true, response = data };
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] WeChatPushLog obj)
        {
            await _baseServicesWeChatPushLog.Dal.Add(obj);
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] WeChatPushLog obj)
        {
            await _baseServicesWeChatPushLog.Dal.Update(obj);
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 删除
        /// </summary> 
        /// <returns></returns> 
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(string id)
        {
            await _baseServicesWeChatPushLog.Dal.DeleteById(id);
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
            await _baseServicesWeChatPushLog.Dal.DeleteByIds(i);
            return new MessageModel<string> { success = true };
        }

    }
}