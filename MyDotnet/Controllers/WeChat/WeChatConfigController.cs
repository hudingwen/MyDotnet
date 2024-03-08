using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Trojan;
using MyDotnet.Domain.Entity.WeChat;
using MyDotnet.Helper;
using MyDotnet.Services;

namespace MyDotnet.Controllers.WeChat
{
    /// <summary>
	/// 微信公众号配置管理
	/// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public partial class WeChatConfigController : Controller
    {
        public BaseServices<WeChatConfig> _baseServicesWeChatConfig;
        public WeChatConfigController(BaseServices<WeChatConfig> baseServicesWeChatConfig)
        {
            _baseServicesWeChatConfig = baseServicesWeChatConfig;
        }


        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="pagination">分页条件</param> 
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<WeChatConfig>>> Get([FromQuery] PaginationModel pagination)
        {
            var whereFind = LinqHelper.True<WeChatConfig>();
            if (!string.IsNullOrEmpty(pagination.key))
            {
                whereFind = whereFind.And(t => t.publicAccount.Contains(pagination.key) || t.publicNick.Contains(pagination.key));
            }
            var data = await _baseServicesWeChatConfig.Dal.QueryPage(whereFind,pagination.page, pagination.size);
            return new MessageModel<PageModel<WeChatConfig>> { success = true, response = data };
        }
        /// <summary>
        /// 获取(id)
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<MessageModel<WeChatConfig>> Get(string id)
        {
            var data = await _baseServicesWeChatConfig.Dal.QueryById(id);
            return new MessageModel<WeChatConfig> { success = true, response = data };
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] WeChatConfig obj)
        {
            await _baseServicesWeChatConfig.Dal.Db.Insertable(obj).ExecuteCommandAsync();
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] WeChatConfig obj)
        {
            await _baseServicesWeChatConfig.Dal.Update(obj);
            return new MessageModel<string> { success = true };
        }

        /// <summary>
        /// 编辑关注
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> putFocus([FromBody] WeChatConfig obj)
        {
            await _baseServicesWeChatConfig.Dal.Update(obj, t => new { t.isFocusReply, t.replyID, t.replyText, t.replyType, t.replyTitle, t.replyDescription });
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 删除
        /// </summary> 
        /// <returns></returns> 
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(string id)
        {
            await _baseServicesWeChatConfig.Dal.DeleteById(id);
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> BatchDelete([FromBody] object[] ids)
        {
            await _baseServicesWeChatConfig.Dal.DeleteByIds(ids);
            return new MessageModel<string> { success = true };
        }

    }
}