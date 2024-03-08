using MyDotnet.Common.Cache;

namespace MyDotnet.Config
{
    public static class CacheConfig
    {
        /// <summary>
        /// 缓存配置
        /// </summary>
        /// <param name="builder"></param>
        public static void SetCache(this WebApplicationBuilder builder)
        {
            //使用内存
            builder.Services.AddMemoryCache();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSingleton<ICaching, Caching>();
        }
    }
}
