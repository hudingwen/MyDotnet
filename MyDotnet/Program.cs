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
            
            //����web��С����
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = 10737418240; // 10GB
            });
            //����С����
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10737418240; // 10GB
            });

            //����
            ConfigHelper.Configuration = builder.Configuration;
            //http������
            builder.Services.AddHttpContextAccessor();
            //����IHttpClientFactory
            builder.Services.AddHttpClient();
            //gb2312�ַ�֧��
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //����������
            builder.Services.AddControllers(o =>
            {
                //������ȫ���쳣����
                o.Filters.Add(typeof(GlobalExceptionsFilter));
            })
                    .ConfigureApiBehaviorOptions(options =>
                    {
                        //���Բ���������֤
                        options.SuppressModelStateInvalidFilter = true;
                    })
                     .AddNewtonsoftJson(options =>
                     {
                         //����ѭ������
                         options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                         //��ʹ���շ���ʽ��key
                         options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                         //����ʱ���ʽ
                         options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                         //����Ϊnull���ֶ�
                         //options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                         //���ñ���ʱ�����UTCʱ��
                         options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                         //����ö�����ַ�Сд
                         options.SerializerSettings.Converters.Add(new StringEnumConverter());
                         //��long����תΪstring
                         options.SerializerSettings.Converters.Add(new LongToStringConverter());
                     });

            //��־����
            builder.SetLog4Net();
            //Swagger����
            builder.SetSwagger();
            //ʵ��ӳ��
            builder.SetAutoMapper();
            //Ȩ�޿���
            builder.SetAuth();
            //���ݿ���
            builder.SetSqlsugar();
            //��������
            builder.SetCache();


            //���ȷ���
            builder.SetQuartz();
            //��ʼ����
            builder.SetHostJob();
            //����ͬ���� ͬ�� IO �������̳߳��̣߳��������ܣ��ر����ڸ߲����¿�������������½����߳̿ݽߡ�
            //builder.Services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true)
            //                .Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);
            //ȡ������������С�������� ���������������ԣ�������һ����ȫ�ԡ�
            builder.WebHost.UseKestrel(o =>
            {
                o.Limits.MinRequestBodyDataRate = null;
            });
             

            var app = builder.Build();
            // Configure the HTTP request pipeline.
            AppHelper.appService = app.Services;
            //����body�ظ���
            app.Use((context, next) =>
            {
                context.Request.EnableBuffering();
                return next(context);
            });
            //�����쳣�м��
            app.UseMiddleware<ExceptionHandlerMiddleware>();

            //ʹ��Swagger
            app.SetSwagger();
            //·��·��ƥ��(������Auth֮ǰ����)
            app.UseRouting();
            //��֤
            app.UseAuthentication();
            //��Ȩ
            app.UseAuthorization();
            // ����·��ע�᣺���� UseEndpoints
            app.MapControllerRoute( name: "default",  pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}