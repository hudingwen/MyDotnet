using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDotnet.Domain.Dto.System
{
    /// <summary>
    /// 泛型消息实体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageModel<T>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int status { get; set; } = 200;
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool success { get; set; } = true;
        /// <summary>
        /// 返回信息
        /// </summary>
        public string msg { get; set; } = string.Empty;
        /// <summary>
        /// 返回数据集合
        /// </summary>
        public T response { get; set; }
        /// <summary>
        /// 返回成功
        /// </summary>
        /// <param name="msg">消息</param>
        /// <returns></returns>
        public static MessageModel<T> Success(string msg)
        {
            return Message(true, msg, default);
        }
        /// <summary>
        /// 返回成功
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="response">数据</param>
        /// <returns></returns>
        public static MessageModel<T> Success(string msg, T response)
        {
            return Message(true, msg, response);
        }
        /// <summary>
        /// 返回失败
        /// </summary>
        /// <param name="msg">消息</param>
        /// <returns></returns>
        public static MessageModel<T> Fail(string msg)
        {
            return Message(false, msg, default);
        }
        /// <summary>
        /// 返回失败
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="response">数据</param>
        /// <returns></returns>
        public static MessageModel<T> Fail(string msg, T response)
        {
            return Message(false, msg, response);
        }
        /// <summary>
        /// 返回消息
        /// </summary>
        /// <param name="success">失败/成功</param>
        /// <param name="msg">消息</param>
        /// <param name="response">数据</param>
        /// <returns></returns>
        public static MessageModel<T> Message(bool success, string msg, T response)
        {
            return new MessageModel<T>() { msg = msg, response = response, success = success };
        }
    }
    /// <summary>
    /// Object消息实体
    /// </summary>
    public class MessageModel
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int status { get; set; } = 200;
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool success { get; set; } = true;
        /// <summary>
        /// 返回信息
        /// </summary>
        public string msg { get; set; } = string.Empty;
        /// <summary>
        /// 返回数据集合
        /// </summary>
        public object response { get; set; }

        /// <summary>
        /// 返回成功
        /// </summary>
        /// <param name="msg">消息</param>
        /// <returns></returns>
        public static MessageModel Success(string msg)
        {
            return Message(true, msg, null);
        }
        /// <summary>
        /// 返回成功
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="response">数据</param>
        /// <returns></returns>
        public static MessageModel Success(string msg, object response)
        {
            return Message(true, msg, response);
        }
        /// <summary>
        /// 返回失败
        /// </summary>
        /// <param name="msg">消息</param>
        /// <returns></returns>
        public static MessageModel Fail(string msg)
        {
            return Message(false, msg, null);
        }
        /// <summary>
        /// 返回失败
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="response">数据</param>
        /// <returns></returns>
        public static MessageModel Fail(string msg, object response)
        {
            return Message(false, msg, response);
        }
        /// <summary>
        /// 返回消息
        /// </summary>
        /// <param name="success">失败/成功</param>
        /// <param name="msg">消息</param>
        /// <param name="response">数据</param>
        /// <returns></returns>
        public static MessageModel Message(bool success, string msg, object response)
        {
            return new MessageModel() { msg = msg, response = response, success = success };
        }
    }
}
