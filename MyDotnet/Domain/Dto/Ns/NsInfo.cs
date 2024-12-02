using MyDotnet.Helper;
using System.Collections.Concurrent;

namespace MyDotnet.Domain.Dto.Ns
{
    /// <summary>
    /// 血糖信息
    /// </summary>
    public static class NsInfo
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

        public static SemaphoreSlim GetLock(string keyId)
        {
            return Locks.GetOrAdd(keyId, _ => new SemaphoreSlim(1, 1));
        }

        public static string KEY = "NSInfo";

        /// <summary>
        /// nginx重载次数
        /// </summary>
        public static int ngCount;

        /// <summary>
        /// ns启动docker镜像源
        /// </summary>
        public static readonly string image = "image";// ConfigHelper.GetValue(new string[] { "nightscout", "image" });
        /// <summary>
        /// 推送公众号id
        /// </summary>
        public static readonly string pushWechatID = "pushWechatID";//ConfigHelper.GetValue(new string[] { "nightscout", "pushWechatID" });
        /// <summary>
        /// 推送公司id
        /// </summary>
        public static readonly string pushCompanyCode = "pushCompanyCode";//ConfigHelper.GetValue(new string[] { "nightscout", "pushCompanyCode" });
        /// <summary>
        /// 推送高低异常消息模板
        /// </summary>
        public static readonly string pushTemplateID_Exception = "pushTemplateID_Exception";// ConfigHelper.GetValue(new string[] { "nightscout", "pushTemplateID_Exception" });
        /// <summary>
        /// 推送持续消息模板
        /// </summary>
        public static readonly string pushTemplateID_Keep = "pushTemplateID_Keep";//ConfigHelper.GetValue(new string[] { "nightscout", "pushTemplateID_Keep" });
        /// <summary>
        /// 推送提醒模板
        /// </summary>
        public static readonly string pushTemplateID_Alert = "pushTemplateID_Alert";//ConfigHelper.GetValue(new string[] { "nightscout", "pushTemplateID_Alert" });
        /// <summary>
        /// 推送模板默认地址
        /// </summary>
        public static readonly string frontPage = "FrontPage";//ConfigHelper.GetValue(new string[] { "nightscout", "FrontPage" });










        /// <summary>
        /// 访问地址模板url
        /// </summary>
        public static readonly string templateUrl = "TemplateUrl";//ConfigHelper.GetValue(new string[] { "nightscout", "TemplateUrl" });
        /// <summary>
        /// 泛域名
        /// </summary>
        public static readonly string genericUrl = "GenericUrl";//ConfigHelper.GetValue(new string[] { "nightscout", "GenericUrl" });
        /// <summary>
        /// ns的MAKER_KEY
        /// </summary>
        public static readonly string MAKER_KEY = "MAKER_KEY";//ConfigHelper.GetValue(new string[] { "nightscout", "MAKER_KEY" });
        /// <summary>
        /// ns的CUSTOM_TITLE
        /// </summary>
        public static readonly string CUSTOM_TITLE = "CUSTOM_TITLE";//ConfigHelper.GetValue(new string[] { "nightscout", "CUSTOM_TITLE" });
        /// <summary>
        /// ns的cer
        /// </summary>
        public static readonly string cer = "cer";//ConfigHelper.GetValue(new string[] { "nightscout", "cer" });
        /// <summary>
        /// ns的key
        /// </summary>
        public static readonly string key = "key";// ConfigHelper.GetValue(new string[] { "nightscout", "key" });
        /// <summary>
        /// ns的推送api
        /// </summary>
        public static readonly string pushUrl = "pushUrl";//ConfigHelper.GetValue(new string[] { "nightscout", "pushUrl" });
        /// <summary>
        /// ns到期提前几天提醒/天
        /// </summary>
        public static readonly string preDays = "preDays";
        /// <summary>
        /// ns到期几天后自动删除实例
        /// </summary>
        public static readonly string afterDays = "afterDays";
        /// <summary>
        /// ns到期提醒内部用户
        /// </summary>
        public static readonly string preInnerUser = "preInnerUser";
        /// <summary>
        /// ns到期检测api查询地址
        /// </summary>
        public static readonly string preCheckUrl = "preCheckUrl";
        /// <summary>
        /// 清理超过期限的ns用户血糖数据 单位/天
        /// </summary>
        public static readonly string cleanOutBlood = "cleanOutBlood";
        /// <summary>
        /// 停止超过期限的ns用户血糖实例 单位/天
        /// </summary>
        public static readonly string stopOutBlood = "stopOutBlood";
        /// <summary>
        /// 微信公众号启动ns关键词
        /// </summary>
        public static readonly string weChatLaunchNsKey = "weChatLaunchNsKey";
        /// <summary>
        /// ns的代理访问配置存放目录
        /// </summary>
        public static readonly string nsNginxCatalog = "nsNginxCatalog";
        /// <summary>
        /// cdn检测错误次数提醒
        /// </summary>
        public static readonly string cdnErrorCount = "cdnErrorCount";
        /// <summary>
        /// cdn间隔时间
        /// </summary>
        public static readonly string cdnErrorSleep = "cdnErrorSleep";
        /// <summary>
        /// cdn检测主动退出
        /// </summary>
        public static readonly string cdnCheckFinish = "cdnCheckFinish";






    }
}
