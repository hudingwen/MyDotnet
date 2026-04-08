using System;
using System.Collections.Generic;
using System.Text;

namespace MyDotnet.Domain.Dto.WeChat
{
    /// <summary>
    /// 微信推送所需信息(公司版本)
    /// </summary>
    public class WeChatUserInfo
    {
        /// <summary>
        /// 微信公众号ID
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 公司代码
        /// </summary>
        public string companyCode { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        public string userID { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string userNick { get; set; }
    }
}
