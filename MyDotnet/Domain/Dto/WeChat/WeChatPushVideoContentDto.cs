using System;
using System.Collections.Generic;
using System.Text;

namespace MyDotnet.Domain.Dto.WeChat
{
    public class WeChatPushVideoContentDto
    {
        /// <summary>
        /// 视频标题
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 视频封面mediaID
        /// </summary>
        public string pictureMediaID { get; set; }
        /// <summary>
        /// 视频mediaID
        /// </summary>
        public string videoMediaID { get; set; }
    }
}
