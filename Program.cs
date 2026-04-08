using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using MyDotnet.Domain.Dto;
using MyDotnet.Helper;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Config;
using System.Text;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using MyDotnet.Domain.Filter;
using MyDotnet.Domain.Middleware;
using Microsoft.AspNetCore.Http.Features;

namespace MyDotnet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            
            //内置web大小限制
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = 10737418240; // 10GB
            });
            //表单大小限制
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10737418240; // 10GB
            });

            //配置
            ConfigHelper.Configuration = builder.Configuration;
            //http上下文
            builder.Services.AddHttpContextAccessor();
            //开启IHttpClientFactory
            builder.Services.AddHttpClient();
            //gb2312字符支持
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
            //实体映射
            builder.SetAutoMapper();
            //权限开启
            builder.SetAuth();
            //数据开启
            builder.SetSqlsugar();
            //开启缓存
            builder.SetCache();


            //调度服务
            builder.SetQuartz();
            //初始任务
            builder.SetHostJob();
            //开启同步读 同步 IO 会阻塞线程池线程，降低性能，特别是在高并发下可能造成吞吐量下降或线程枯竭。
            //builder.Services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true)
            //                .Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);
            //取消请求正文最小数据速率 提高慢速请求兼容性，但降低一定安全性。
            builder.WebHost.UseKestrel(o =>
            {
                o.Limits.MinRequestBodyDataRate = null;
            });
             

            var app = builder.Build();
            // Configure the HTTP request pipeline.
            AppHelper.appService = app.Services;
            //开启body重复读
            app.Use((context, next) =>
            {
                context.Request.EnableBuffering();
                return next(context);
            });
            //开启异常中间件
            app.UseMiddleware<ExceptionHandlerMiddleware>();

            //使用Swagger
            app.SetSwagger();
            //路由路由匹配(必须在Auth之前调用)
            app.UseRouting();
            //认证
            app.UseAuthentication();
            //授权
            app.UseAuthorization();
            // 顶层路由注册：代替 UseEndpoints
            app.MapControllerRoute( name: "default",  pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}