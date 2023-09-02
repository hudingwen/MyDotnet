using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using MyDotnet.Filter;
using MyDotnet.Domain.Dto;
using MyDotnet.Helper;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Config;
using System.Text;

namespace MyDotnet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            //配置
            ConfigHelper.Configuration = builder.Configuration;
            //http上下文
            builder.Services.AddHttpContextAccessor();
            //开启IHttpClientFactory
            builder.Services.AddHttpClient();
            //gb2312支持
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //控制器配置
            builder.Services.AddControllers(o =>
            {
                //控制器全局异常捕获
                o.Filters.Add(typeof(GlobalExceptionsFilter));
            })
                    .ConfigureApiBehaviorOptions(options =>
                    {
                        //忽略参数必填验证
                        options.SuppressModelStateInvalidFilter = true;
                    })
                     .AddNewtonsoftJson(options =>
                     {
                         //忽略循环引用
                         options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                         //不使用驼峰样式的key
                         options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                         //设置时间格式
                         options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                         //忽略为null的字段
                         //options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                         //设置本地时间而非UTC时间
                         options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                         //设置枚举首字符小写
                         options.SerializerSettings.Converters.Add(new StringEnumConverter());
                         //将long类型转为string
                         options.SerializerSettings.Converters.Add(new LongToStringConverter());
                     });

            //日志开启
            builder.SetLog4Net();
            //Swagger开启
            builder.SetSwagger();
            //数据开启
            builder.SetSqlsugar();
            //调度服务
            builder.SetQuartz();
            //实体映射
            builder.SetAutoMapper();
            //权限开启
            builder.SetAuth();
            //初始任务
            builder.SetHostJob();






            var app = builder.Build();
            // Configure the HTTP request pipeline.
            //使用Swagger
            app.SetSwagger();
            //开启body重复读
            //app.Use((context, next) =>
            //{
            //    context.Request.EnableBuffering();
            //    return next(context);
            //});
            //路由路由匹配(必须在Auth之前调用)
            app.UseRouting();
            //认证
            app.UseAuthentication();
            //授权
            app.UseAuthorization();
            //路由端点
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.Run();
        }
    }
}