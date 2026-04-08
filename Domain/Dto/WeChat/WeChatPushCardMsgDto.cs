using System;
using System.Collections.Generic;
using System.Text;

namespace MyDotnet.Domain.Dto.WeChat
{
    /// <summary>
    /// 推送给微信所需Dto
    /// </summary>
    public class WeChatPushCardMsgDto
    {
        /// <summary>
        /// 推送微信用户ID
        /// </summary>
        public string touser { get; set; }
        /// <summary>
        /// 推送的模板ID
        /// </summary>
        public string template_id { get; set; }
        /// <summary>
        /// 推送URL地址
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 推送的数据
        /// </summary>
        public WeChatPushCardMsgDetailDto data { get; set; }
        /// <summary>
        /// 小程序
        /// </summary>
        public WeChatCardMsgMiniprogram miniprogram { get; set; }
    }
}
