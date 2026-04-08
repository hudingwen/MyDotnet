using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Ns
{
    /// <summary>
    /// app历史版本记录
    /// </summary>
    public class AppShopLog : BaseEntity
    {

        /// <summary>
        /// app版本父级id
        /// </summary>
        [SugarColumn( IsNullable = false)]
        public long appPid { get; set; }
        /// <summary>
        /// app名称
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string appName { get; set; }
        /// <summary>
        /// app描述
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string appDescription { get; set; }
        /// <summary>
        /// app当前版本 1.0.0
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string appVersion { get; set; }
        /// <summary>
        /// app平台 1-安卓 2-苹果
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true,DefaultValue = "1")]
        public string appPlatform { get; set; } 
        /// <summary>
        /// app包路径 com.android.loop
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string appPackage { get; set; }
    }
}
