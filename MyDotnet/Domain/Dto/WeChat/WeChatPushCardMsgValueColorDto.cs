using System;
using System.Collections.Generic;
using System.Text;

namespace MyDotnet.Domain.Dto.WeChat
{
    /// <summary>
    /// 微信keyword所需Dto
    /// </summary>
    public class WeChatPushCardMsgValueColorDto
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 文字颜色
        /// </summary>
        public string color { get; set; } = "#173177";
    }
}
