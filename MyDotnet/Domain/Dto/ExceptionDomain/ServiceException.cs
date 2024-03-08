using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDotnet.Domain.Dto.ExceptionDomain
{
    /// <summary>
    /// 服务异常
    /// </summary>
    public class ServiceException : Exception
    {
        /// <summary>
        /// 异常消息
        /// </summary>
        /// <param name="message"></param>
        public ServiceException(string message) : base(message)
        {

        }
        /// <summary>
        /// 异常消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="ex">异常</param>
        public ServiceException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
