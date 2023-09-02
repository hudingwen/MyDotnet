
using MyDotnet.Domain.Dto.System;

namespace MyDotnet.Config
{
    /// <summary>
    /// Automapper 启动服务
    /// </summary>
    public static class AutoMapperConfig
    {
        /// <summary>
        /// 实现实体映射配置
        /// </summary>
        /// <param name="builder"></param>
        public static void SetAutoMapper(this WebApplicationBuilder builder)
        {
            builder.Services.AddAutoMapper(typeof(AutoMapperRegister));
        }
    }
}
