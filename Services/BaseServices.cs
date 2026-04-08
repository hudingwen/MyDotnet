using MyDotnet.Repository;

namespace MyDotnet.Services
{
    /// <summary>
    /// 服务层
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseServices<T> where T : class, new()
    {
        /// <summary>
        /// 仓储层实例
        /// </summary>
        public BaseRepository<T> Dal { get; set; }
        /// <summary>
        /// 服务层构造函数
        /// </summary>
        /// <param name="baseRepository"></param>
        public BaseServices(BaseRepository<T> baseRepository)
        {
            Dal = baseRepository;
        }

    }
}
