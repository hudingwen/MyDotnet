using System;
using System.Collections.Generic;
using System.Text;

namespace MyDotnet.Domain.Dto.WeChat
{
    /// <summary>
    /// 微信推送消息Dto
    /// </summary>
    public class WeChatCardMsgDataDto
    {
        /// <summary>
        /// 推送关键信息
        /// </summary>
        public WeChatUserInfo info { get; set; }
        /// <summary>
        /// 推送卡片消息Dto
        /// </summary>
        public WeChatCardMsgDetailDto cardMsg { set; get; }
        /// <summary>
        /// 是否为微信新版消息模板
        /// </summary>
        public bool isNewVersion { get; set; } = false;
        /// <summary>
        /// 新版的消息模型
        ///  格式如下
        ///  {
        /// 	"thing8": {
        /// 		"value": "张三"
        /// 	},
        /// 	"time2": {
        /// 		"value": "2022年11月"
        /// 	},
        /// 	"thing7": {
        /// 		"value": "管理组班表/区域组班表/营运组班表"
        /// 	},
        /// 	"time9": {
        /// 		"value": "2019年10月1日 15:01"
        /// 	},
        /// 	"thing4": {
        /// 		"value": "矿工1天;迟到2次;早退3次;"
        /// 	}
        ///  }
        /// </summary>
        public object newMsg { set; get; }
    }
}
