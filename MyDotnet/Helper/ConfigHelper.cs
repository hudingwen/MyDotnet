namespace MyDotnet.Helper
{
    /// <summary>
    /// 配置文件帮助类
    /// </summary>
    public static class ConfigHelper
    {
        /// <summary>
        /// 全局配置文件实例
        /// </summary>
        public static IConfiguration Configuration { get; set; }
        /// <summary>
        /// 获取配置文件
        /// 如果常用的配置可以考虑静态变量存储
        /// </summary>
        /// <param name="sections">节点配置</param>
        /// <returns></returns>
        public static string GetValue(params string[] sections)
        {
            return Configuration[string.Join(":", sections)];
        }

        /// <summary>
        /// 获取配置文件集合对象数组
        /// 如果常用的配置可以考虑静态变量存储
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sections"></param>
        /// <returns></returns>
        public static List<T> GetList<T>(params string[] sections)
        {
            List<T> list = new List<T>();
            // 引用 Microsoft.Extensions.Configuration.Binder 包
            Configuration.Bind(string.Join(":", sections), list);
            return list;
        }
    }
}
