using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Dto.WeChat;
using MyDotnet.Domain.Entity.WeChat;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.WeChat;

namespace MyDotnet.Controllers.WeChat
{
    /// <summary>
	/// 微信关键词管理
	/// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public partial class WeChatKeywordController : Controller
    {
        public BaseServices<WeChatKeyword> _wechatKeywordServices;
        public WeChatConfigServices _weChatConfigServices;

        public WeChatKeywordController(BaseServices<WeChatKeyword> wechatKeywordServices, WeChatConfigServices weChatConfigServices)
        {
            _wechatKeywordServices = wechatKeywordServices;
            _weChatConfigServices = weChatConfigServices;
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="pagination">分页条件</param> 
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<WeChatKeyword>>> Get([FromQuery] PaginationModel pagination)
        {
            pagination.orderByFileds = "Id desc";
            var data = await _wechatKeywordServices.Dal.QueryPage(pagination);
            return new MessageModel<PageModel<WeChatKeyword>> { success = true, response = data };
        }
        /// <summary>
        /// 获取(id)
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<MessageModel<WeChatKeyword>> Get(string id)
        {
            var data = await _wechatKeywordServices.Dal.QueryById(id);
            return new MessageModel<WeChatKeyword> { success = true, response = data };
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] WeChatKeyword obj)
        {
            await _wechatKeywordServices.Dal.Add(obj);
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] WeChatKeyword obj)
        {
            await _wechatKeywordServices.Dal.Update(obj);
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 删除
        /// </summary> 
        /// <returns></returns> 
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(string id)
        {
            await _wechatKeywordServices.Dal.DeleteById(id);
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> BatchDelete([FromBody] object[] ids)
        {
            await _wechatKeywordServices.Dal.DeleteByIds(ids);
            return new MessageModel<string> { success = true };
        }
        /// <summary>
        /// 上传微信公众号文档
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<WeChatApiDto>> UpdateWeChatFile([FromQuery] string id, string type, IFormCollection form)
        {

            var res = await _weChatConfigServices.GetToken(id);

            var data = await WeChatHelper.UploadMedia(res.response.access_token, type, form);
            if (data.errcode.Equals(0))
            {
                if ("video".Equals(type))
                {
                    var info = await WeChatHelper.GetMediaInfo(res.response.access_token, data.media_id);
                    data.url = info.down_url;
                }
                return MessageModel<WeChatApiDto>.Success("上传成功", data);
            }
            else
            {
                return MessageModel<WeChatApiDto>.Fail($"上传失败:{data.errmsg}", data);
            }

        }
        /// <summary>
        /// 获取微信公众号文档
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<WeChatApiDto>> GetWeChatMediaList([FromQuery] string id, string type = "image", int page = 1, int size = 10)
        {

            var res = await _weChatConfigServices.GetToken(id);

            var data = await WeChatHelper.GetMediaList(res.response.access_token, type, page, size);
            if (data.errcode.Equals(0))
            {
                if ("video".Equals(type))
                {
                    foreach (var item in data.item)
                    {
                        var info = await WeChatHelper.GetMediaInfo(res.response.access_token, item.media_id);
                        item.url = data.url = info.down_url;
                    }
                }
                return MessageModel<WeChatApiDto>.Success("获取成功", data);
            }
            else
            {
                return MessageModel<WeChatApiDto>.Fail($"获取失败:{data.errmsg}", data);
            }

        }



    }
}