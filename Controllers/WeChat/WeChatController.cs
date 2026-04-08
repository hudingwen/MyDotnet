
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Dto.WeChat;
using MyDotnet.Services.WeChat;
using System.Text;

namespace MyDotnet.Controllers.WeChat
{
    /// <summary>
    /// 微信公众号接口管理 
    /// </summary>   
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public partial class WeChatController : Controller
    {
        public WeChatConfigServices _weChatConfigServices;
        public WeChatController(WeChatConfigServices weChatConfigServices)
        {
            _weChatConfigServices = weChatConfigServices;
        }

        /// <summary>
        /// 入口
        /// </summary>
        /// <param name="validDto"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [HttpGet]
        public async Task<string> Valid([FromQuery] WeChatValidDto validDto)
        {
            var result = await Request.BodyReader.ReadAsync();
            var str = Encoding.UTF8.GetString(result.Buffer);
            //byte[] postContent = new byte[(int)Request.ContentLength];
            //await Request.Body.ReadAsync(postContent, 0, postContent.Length);
            //string str = Encoding.UTF8.GetString(postContent);
            return await _weChatConfigServices.Valid(validDto, str);
        }
        /// <summary>
        /// 更新Token
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
        [HttpGet]
        public async Task<MessageModel<WeChatApiDto>> GetToken(string id)
        {
            return await _weChatConfigServices.GetToken(id);

        }
        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
        [HttpGet]
        public async Task<MessageModel<WeChatApiDto>> RefreshToken(string id)
        {
            return await _weChatConfigServices.RefreshToken(id);

        }
        /// <summary>
        /// 获取模板
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
        [HttpGet]
        public async Task<MessageModel<WeChatApiDto>> GetTemplate(string id)
        {
            return await _weChatConfigServices.GetTemplate(id);
        }
        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
        [HttpGet]
        public async Task<MessageModel<WeChatApiDto>> GetMenu(string id)
        {
            return await _weChatConfigServices.GetMenu(id);
        }

        /// <summary>
        /// 更新菜单
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns> 
        [HttpPut]
        public async Task<MessageModel<WeChatApiDto>> UpdateMenu(WeChatApiDto menu)
        {
            return await _weChatConfigServices.UpdateMenu(menu);
        }
        /// <summary>
        /// 获取订阅用户(所有)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<WeChatApiDto>> GetSubUsers(string id)
        {
            return await _weChatConfigServices.GetSubUsers(id);
        }
        /// <summary>
        /// 获取订阅用户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="openid"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatApiDto>> GetSubUser(string id, string openid)
        {
            return await _weChatConfigServices.GetSubUser(id, openid);
        }
        /// <summary>
        /// 获取一个绑定员工公众号二维码
        /// </summary>
        /// <param name="info">消息</param> 
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatResponseUserInfo>> GetQRBind([FromQuery] WeChatUserInfo info)
        {
            return await _weChatConfigServices.GetQRBind(info);
        }
        /// <summary>
        /// 推送卡片消息接口
        /// </summary>
        /// <param name="msg">卡片消息对象</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatResponseUserInfo>> PushCardMsg(WeChatCardMsgDataDto msg)
        {
            return await _weChatConfigServices.PushCardMsg(msg);
        }
        /// <summary>
        /// 推送卡片消息接口
        /// </summary>
        /// <param name="msg">卡片消息对象</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatResponseUserInfo>> PushCardMsgGet([FromQuery] WeChatCardMsgDataDto msg)
        {
            return await _weChatConfigServices.PushCardMsg(msg);
        }
        /// <summary>
        /// 推送文本消息
        /// </summary>
        /// <param name="msg">消息对象</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatApiDto>> PushTxtMsg([FromBody] WeChatPushTestDto msg)
        {
            return await _weChatConfigServices.PushTxtMsg(msg);
        }
        /// <summary>
        /// 通过绑定用户获取微信用户信息(一般用于初次绑定检测)
        /// </summary>
        /// <param name="info">信息</param> 
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatResponseUserInfo>> GetBindUserInfo([FromQuery] WeChatUserInfo info)
        {
            return await _weChatConfigServices.GetBindUserInfo(info);
        }
        /// <summary>
        /// 用户解绑
        /// </summary>
        /// <param name="info">消息</param> 
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatResponseUserInfo>> UnBind([FromQuery] WeChatUserInfo info)
        {
            return await _weChatConfigServices.UnBind(info);
        }
    }
}
