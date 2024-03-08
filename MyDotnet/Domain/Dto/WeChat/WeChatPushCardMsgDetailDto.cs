using System;
using System.Collections.Generic;
using System.Text;

namespace MyDotnet.Domain.Dto.WeChat
{
    /// <summary>
    /// 推送详细数据
    /// </summary>
    public class WeChatPushCardMsgDetailDto
    {
        public WeChatPushCardMsgValueColorDto first { get; set; }
        public WeChatPushCardMsgValueColorDto keyword1 { get; set; }
        public WeChatPushCardMsgValueColorDto keyword2 { get; set; }
        public WeChatPushCardMsgValueColorDto keyword3 { get; set; }
        public WeChatPushCardMsgValueColorDto keyword4 { get; set; }
        public WeChatPushCardMsgValueColorDto keyword5 { get; set; }
        public WeChatPushCardMsgValueColorDto remark { get; set; }
    }
}
