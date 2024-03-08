namespace MyDotnet.Config
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// Swagger配置类
    /// </summary>
    public static class SwaggerConfig
    {
        /// <summary>
        /// 添加Swagger服务
        /// </summary>
        /// <param name="builder"></param>
        public static void SetSwagger(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(options =>
            {
                //首页描述配置
                options.SwaggerDoc("v1", new OpenApiInfo
                {

                    Version = "v1",
                    Title = "我的api",
                    Description = "这是由dotnet生成的接口文档",
                    //TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "我的网站",
                        Url = new Uri("https://example.com/contact")
                    },
                    //License = new OpenApiLicense
                    //{
                    //    Name = "我的license",
                    //    Url = new Uri("https://example.com/license")
                    //}
                });

                // 开启小锁
                options.OperationFilter<AddResponseHeadersFilter>();
                options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                // 在header中添加token，传递到后台
                options.OperationFilter<SecurityRequirementsOperationFilter>();

                //配置认证说明文档
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT授权,输入你的token, 格式为: Bearer 你的token",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey
                });

                //配置api注释 
                var xmlPathApi = Path.Combine(AppContext.BaseDirectory, $"MyDotnet.xml");
                if (File.Exists(xmlPathApi))
                    options.IncludeXmlComments(xmlPathApi, true);


            });

        }

        /// <summary>
        /// 使用Swagger
        /// </summary>
        /// <param name="app"></param>
        public static void SetSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });

        }
    }
}


