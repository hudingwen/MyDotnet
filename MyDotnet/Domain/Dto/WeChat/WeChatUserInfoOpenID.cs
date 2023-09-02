using System;
using System.Collections.Generic;
using System.Text;

namespace MyDotnet.Domain.Dto.WeChat
{
    /// <summary>
    /// 微信推送所需信息(OpenID版本)
    /// </summary>
    public class WeChatUserInfoOpenID
    {
        /// <summary>
        /// 微信公众号ID
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 微信OpenID
        /// </summary>
        public List<string> userID { get; set; }
    }
}
