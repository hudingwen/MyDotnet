

using MyDotnet.Domain.Dto.System;
using MyDotnet.Helper;

namespace MyDotnet.Domain.Middleware
{
    /// <summary>
    /// 全局异常中间件
    /// </summary>
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                LogHelper.logSys.Error("系统错误",ex);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception e)
        {
            switch (e)
            {
                case UnauthorizedAccessException:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    break;
                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            context.Response.ContentType = "application/json";

            await context.Response
                .WriteAsync(JsonHelper.ObjToJson(MessageModel.Fail($"系统错误:{e.Message}")))
                .ConfigureAwait(false);
        }
    }
}