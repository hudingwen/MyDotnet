using MyDotnet.Helper;

namespace MyDotnet.Domain.Dto.Ns
{
    /// <summary>
    /// 血糖信息
    /// </summary>
    public static class NsInfo
    {

        /// <summary>
        /// ns启动docker镜像源
        /// </summary>
        public static readonly string image = ConfigHelper.GetValue(new string[] { "nightscout", "image" });
        /// <summary>
        /// 推送公众号id
        /// </summary>
        public static readonly string pushWechatID = ConfigHelper.GetValue(new string[] { "nightscout", "pushWechatID" });
        /// <summary>
        /// 推送公司id
        /// </summary>
        public static readonly string pushCompanyCode = ConfigHelper.GetValue(new string[] { "nightscout", "pushCompanyCode" });
        /// <summary>
        /// 推送高低异常消息模板
        /// </summary>
        public static readonly string pushTemplateID_Exception = ConfigHelper.GetValue(new string[] { "nightscout", "pushTemplateID_Exception" });
        /// <summary>
        /// 推送持续消息模板
        /// </summary>
        public static readonly string pushTemplateID_Keep = ConfigHelper.GetValue(new string[] { "nightscout", "pushTemplateID_Keep" });
        /// <summary>
        /// 推送提醒模板
        /// </summary>
        public static readonly string pushTemplateID_Alert =  ConfigHelper.GetValue(new string[] { "nightscout", "pushTemplateID_Alert" });
        /// <summary>
        /// 推送模板默认地址
        /// </summary>
        public static readonly string frontPage = ConfigHelper.GetValue(new string[] { "nightscout", "FrontPage" });
        /// <summary>
        /// 小程序id
        /// </summary>
        public static readonly string miniAppid = ConfigHelper.GetValue(new string[] { "miniProgram", "appid" });
        /// <summary>
        /// 小程序密钥
        /// </summary>
        public static readonly string miniSecret = ConfigHelper.GetValue(new string[] { "miniProgram", "secret" });
        /// <summary>
        /// 小程序发布环境
        /// </summary>
        public static readonly string miniEnv = ConfigHelper.GetValue(new string[] { "miniProgram", "env" });
        /// <summary>
        /// 小程序访问mongo的地址
        /// </summary>
        public static readonly string miniHost = ConfigHelper.GetValue(new string[] { "miniProgram", "Host" });
        /// <summary>
        /// 小程序访问mongo的端口
        /// </summary>
        public static readonly string miniPort = ConfigHelper.GetValue(new string[] { "miniProgram", "Port" });
        /// <summary>
        /// 小程序访问mongo的登录账号
        /// </summary>
        public static readonly string miniLoginName = ConfigHelper.GetValue(new string[] { "miniProgram", "LoginName" });
        /// <summary>
        /// 小程序访问mongo的登录密码
        /// </summary>
        public static readonly string miniLoginPasswd = ConfigHelper.GetValue(new string[] { "miniProgram", "LoginPasswd" });
        /// <summary>
        /// 小程序授权方式
        /// </summary>
        public static readonly string miniGrantType = "client_credential";
        /// <summary>
        /// 小程序路径
        /// </summary>
        public static readonly string miniPath = ConfigHelper.GetValue(new string[] { "miniProgram", "path" });
        /// <summary>
        /// ns插件列表
        /// </summary>
        public static readonly List<NSPlugin> plugins = ConfigHelper.GetList<NSPlugin>(new string[] { "nightscout", "plugins" });
        /// <summary>
        /// 小程序名言
        /// </summary>
        public static readonly List<string> sayings = ConfigHelper.GetList<string>(new string[] { "miniProgram", "sayings" });
        /// <summary>
        /// 小程序名称
        /// </summary>
        public static readonly string title = ConfigHelper.GetValue(new string[] { "miniProgram", "title" });
        /// <summary>
        /// 访问地址模板url
        /// </summary>
        public static readonly string templateUrl = ConfigHelper.GetValue(new string[] { "nightscout", "TemplateUrl" });
        /// <summary>
        /// ns的MAKER_KEY
        /// </summary>
        public static readonly string MAKER_KEY = ConfigHelper.GetValue(new string[] { "nightscout", "MAKER_KEY" });
        /// <summary>
        /// ns的CUSTOM_TITLE
        /// </summary>
        public static readonly string CUSTOM_TITLE = ConfigHelper.GetValue(new string[] { "nightscout", "CUSTOM_TITLE" });
        /// <summary>
        /// ns的cer
        /// </summary>
        public static readonly string cer = ConfigHelper.GetValue(new string[] { "nightscout", "cer" });
        /// <summary>
        /// ns的key
        /// </summary>
        public static readonly string key = ConfigHelper.GetValue(new string[] { "nightscout", "key" });
        /// <summary>
        /// ns的推送api
        /// </summary>
        public static readonly string pushUrl = ConfigHelper.GetValue(new string[] { "nightscout", "pushUrl" });

        public static readonly string apKeyID = ConfigHelper.GetValue(new string[] { "appleRemote", "apKeyID" }).ObjToString();
        public static readonly string apKey = ConfigHelper.GetValue(new string[] { "appleRemote", "apKey" }).ObjToString();
        public static readonly string apTeamID = ConfigHelper.GetValue(new string[] { "appleRemote", "teamID" }).ObjToString();
        public static readonly string apEnv = ConfigHelper.GetValue(new string[] { "appleRemote", "env" }).ObjToString();

        /// <summary>
        /// cdf操作key
        /// </summary>
        public static readonly string cfKey = ConfigHelper.GetValue(new string[] { "cf", "key" }).ObjToString();
        /// <summary>
        /// cf操作域名
        /// </summary>
        public static readonly string cfZoomID = ConfigHelper.GetValue(new string[] { "cf", "zoomID" }).ObjToString();

        /// <summary>
        /// 默认cdn
        /// </summary>
        public static readonly string defaultCND = ConfigHelper.GetValue(new string[] { "cf", "defaultCND" }).ObjToString();
        /// <summary>
        /// 亚马逊cdn
        /// </summary>
        public static readonly string cdnAws = ConfigHelper.GetValue(new string[] { "cf", "cdnAws" }).ObjToString();
        /// <summary>
        /// 七牛云cdn
        /// </summary>
        public static readonly string cdnQiniu = ConfigHelper.GetValue(new string[] { "cf", "cdnQiniu" }).ObjToString();
        /// <summary>
        /// 阿里云cdn
        /// </summary>
        public static readonly string cdnAliyun = ConfigHelper.GetValue(new string[] { "cf", "cdnAliyun" }).ObjToString();
        /// <summary>
        /// 腾讯云cdn
        /// </summary>
        public static readonly string cdnQclound = ConfigHelper.GetValue(new string[] { "cf", "cdnQclound" }).ObjToString();

    }
}
