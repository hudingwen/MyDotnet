using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Ns
{
    /// <summary>
    /// 商店app信息表
    /// </summary>
    public class AppShopInfo : BaseEntity
    {

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
        /// <summary>
        /// app地址
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false)]
        public string appUrl { get; set; }
        /// <summary>
        /// app图标
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false)]
        public string appIcon { get; set; }
    }
}
