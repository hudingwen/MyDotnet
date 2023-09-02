using System;
using System.Collections.Generic;
using System.Text;

namespace MyDotnet.Domain.Dto.WeChat
{
    /// <summary>
    /// 微信推送消息Dto
    /// </summary>
    public class WeChatCardMsgDataOpenIDDto
    {
        /// <summary>
        /// 推送关键信息
        /// </summary>
        public WeChatUserInfoOpenID info { get; set; }
        /// <summary>
        /// 推送卡片消息Dto
        /// </summary>
        public WeChatCardMsgDetailDto cardMsg { set; get; }
    }
}
