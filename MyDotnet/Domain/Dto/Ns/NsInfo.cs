using MyDotnet.Helper;

namespace MyDotnet.Domain.Dto.Ns
{
    /// <summary>
    /// 血糖信息
    /// </summary>
    public static class NsInfo
    {
        /// <summary>
        /// 推送公众号id
        /// </summary>
        public static string pushWechatID = ConfigHelper.GetValue(new string[] { "nightscout", "pushWechatID" });
        /// <summary>
        /// 推送公司id
        /// </summary>
        public static string pushCompanyCode = ConfigHelper.GetValue(new string[] { "nightscout", "pushCompanyCode" });
        /// <summary>
        /// 推送高低异常消息模板
        /// </summary>

        public static string pushTemplateID_Exception = ConfigHelper.GetValue(new string[] { "nightscout", "pushTemplateID_Exception" });
        /// <summary>
        /// 推送持续消息模板
        /// </summary>
        public static string pushTemplateID_Keep = ConfigHelper.GetValue(new string[] { "nightscout", "pushTemplateID_Keep" });
        /// <summary>
        /// 推送提醒模板
        /// </summary>
        public static string pushTemplateID_Alert =  ConfigHelper.GetValue(new string[] { "nightscout", "pushTemplateID_Alert" });
        /// <summary>
        /// 推送模板默认地址
        /// </summary>
        public static string frontPage = ConfigHelper.GetValue(new string[] { "nightscout", "FrontPage" });
        /// <summary>
        /// 小程序id
        /// </summary>
        public static string miniAppid = ConfigHelper.GetValue(new string[] { "miniProgram", "appid" });
        /// <summary>
        /// 小程序密钥
        /// </summary>
        public static string miniSecret = ConfigHelper.GetValue(new string[] { "miniProgram", "secret" });
        /// <summary>
        /// 小程序发布环境
        /// </summary>
        public static string miniEnv = ConfigHelper.GetValue(new string[] { "miniProgram", "env" });
        /// <summary>
        /// 小程序访问mongo的地址
        /// </summary>
        public static string miniHost = ConfigHelper.GetValue(new string[] { "miniProgram", "Host" });
        /// <summary>
        /// 小程序访问mongo的端口
        /// </summary>
        public static string miniPort = ConfigHelper.GetValue(new string[] { "miniProgram", "Port" });
        /// <summary>
        /// 小程序访问mongo的登录账号
        /// </summary>
        public static string miniLoginName = ConfigHelper.GetValue(new string[] { "miniProgram", "LoginName" });
        /// <summary>
        /// 小程序访问mongo的登录密码
        /// </summary>
        public static string miniLoginPasswd = ConfigHelper.GetValue(new string[] { "miniProgram", "LoginPasswd" });
        /// <summary>
        /// 小程序授权方式
        /// </summary>
        public static string miniGrantType = "client_credential";
        /// <summary>
        /// 小程序路径
        /// </summary>
        public static string miniPath = ConfigHelper.GetValue(new string[] { "miniProgram", "path" });
        /// <summary>
        /// ns插件列表
        /// </summary>
        public static List<NSPlugin> plugins = ConfigHelper.GetList<NSPlugin>(new string[] { "nightscout", "plugins" });
        /// <summary>
        /// 小程序名言
        /// </summary>

        public static List<string> sayings = ConfigHelper.GetList<string>(new string[] { "miniProgram", "sayings" });
        /// <summary>
        /// 小程序名称
        /// </summary>

        public static string title = ConfigHelper.GetValue(new string[] { "miniProgram", "title" });
        /// <summary>
        /// 访问地址模板url
        /// </summary>

        public static string templateUrl = ConfigHelper.GetValue(new string[] { "nightscout", "TemplateUrl" });
        /// <summary>
        /// ns的MAKER_KEY
        /// </summary>
        public static string MAKER_KEY = ConfigHelper.GetValue(new string[] { "nightscout", "MAKER_KEY" });
        /// <summary>
        /// ns的CUSTOM_TITLE
        /// </summary>
        public static string CUSTOM_TITLE = ConfigHelper.GetValue(new string[] { "nightscout", "CUSTOM_TITLE" });
        /// <summary>
        /// ns的cer
        /// </summary>
        public static string cer = ConfigHelper.GetValue(new string[] { "nightscout", "cer" });
        /// <summary>
        /// ns的key
        /// </summary>
        public static string key = ConfigHelper.GetValue(new string[] { "nightscout", "key" });
        /// <summary>
        /// ns的推送api
        /// </summary>
        public static string pushUrl = ConfigHelper.GetValue(new string[] { "nightscout", "pushUrl" });

        public static string apKeyID = ConfigHelper.GetValue(new string[] { "appleRemote", "apKeyID" }).ObjToString();
        public static string apKey = ConfigHelper.GetValue(new string[] { "appleRemote", "apKey" }).ObjToString();
        public static string apTeamID = ConfigHelper.GetValue(new string[] { "appleRemote", "teamID" }).ObjToString();
        public static string apEnv = ConfigHelper.GetValue(new string[] { "appleRemote", "env" }).ObjToString();


        public static string cfKey = ConfigHelper.GetValue(new string[] { "cf", "key" }).ObjToString();
        public static string cfZoomID = ConfigHelper.GetValue(new string[] { "cf", "zoomID" }).ObjToString();
        public static string cfCDN = ConfigHelper.GetValue(new string[] { "cf", "cdn" }).ObjToString();

    }
}
