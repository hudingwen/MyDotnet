using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyDotnet.Domain.Dto.ExceptionDomain;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Helper;

namespace MyDotnet.Domain.Filter
{
    /// <summary>
    /// 全局异常错误日志
    /// </summary>
    public class GlobalExceptionsFilter : IExceptionFilter
    {
        /// <summary>
        /// 异常捕获
        /// </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
           

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;

            var res = new ContentResult();
            if (context.Exception is ServiceException)
            {
                LogHelper.logSys.Error("业务异常", context.Exception);
                res.Content = JsonHelper.ObjToJson(MessageModel.Fail($"业务异常:{context.Exception.Message}"));
                context.Result = res;
            }
            else
            {
                LogHelper.logSys.Error("系统错误", context.Exception);
                res.Content = JsonHelper.ObjToJson(MessageModel.Fail($"系统错误:{context.Exception.Message}"));
                context.Result = res;
            }
        }


    }
}
