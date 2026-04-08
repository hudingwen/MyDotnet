
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Domain.Dto.System;

namespace MyDotnet.Controllers.Base
{
    /// <summary>
    /// 控制器基础类
    /// </summary>
    public class BaseApiController : Controller
    {
        /// <summary>
        /// 返回成功
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        [NonAction]
        public MessageModel<T> Success<T>(T data, string msg = "成功")
        {
            return new MessageModel<T>()
            {
                success = true,
                msg = msg,
                response = data,
            };
        }
        /// <summary>
        /// 返回失败
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [NonAction]
        public MessageModel<string> Success(string msg = "成功")
        {
            return new MessageModel<string>()
            {
                success = true,
                status = 200,
                msg = msg,
                response = null,
            };
        }

        /// <summary>
        /// 返回失败
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [NonAction]
        public MessageModel<string> Failed(string msg = "失败", int status = 200)
        {
            return new MessageModel<string>()
            {
                success = false,
                status = status,
                msg = msg
            };
        }
        /// <summary>
        /// 返回成功
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        [NonAction]
        public MessageModel<T> Failed<T>(T data, string msg = "成功")
        {
            return new MessageModel<T>()
            {
                success = true,
                msg = msg,
                response = data,
            };
        }
        /// <summary>
        /// 返回失败
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [NonAction]
        public MessageModel<T> Failed<T>(string msg = "失败", int status = 200)
        {
            return new MessageModel<T>()
            {
                success = false,
                status = status,
                msg = msg
            };
        }
        /// <summary>
        /// 返回分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="page"></param>
        /// <param name="dataCount"></param>
        /// <param name="pageSize"></param>
        /// <param name="data"></param>
        /// <param name="pageCount"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        [NonAction]
        public MessageModel<PageModel<T>> SuccessPage<T>(int page, int dataCount, int pageSize, List<T> data,
            int pageCount, string msg = "获取成功")
        {
            return new MessageModel<PageModel<T>>()
            {
                success = true,
                msg = msg,
                response = new PageModel<T>(page, dataCount, pageSize, data)
            };
        }

        [NonAction]
        public MessageModel<PageModel<T>> SuccessPage<T>(PageModel<T> pageModel, string msg = "获取成功")
        {
            return new MessageModel<PageModel<T>>()
            {
                success = true,
                msg = msg,
                response = pageModel
            };
        }
    }
}