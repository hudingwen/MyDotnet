﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MyDotnet.Helper
{
    public class StringHelper
    {
        /// <summary>
        /// 根据分隔符返回前n条数据
        /// </summary>
        /// <param name="content">数据内容</param>
        /// <param name="separator">分隔符</param>
        /// <param name="top">前n条</param>
        /// <param name="isDesc">是否倒序（默认false）</param>
        /// <returns></returns>
        public static List<string> GetTopDataBySeparator(string content, string separator, int top, bool isDesc = false)
        {
            if (string.IsNullOrEmpty(content))
            {
                return new List<string>() { };
            }

            if (string.IsNullOrEmpty(separator))
            {
                throw new ArgumentException("message", nameof(separator));
            }

            var dataArray = content.Split(separator).Where(d => !string.IsNullOrEmpty(d)).ToArray();
            if (isDesc)
            {
                Array.Reverse(dataArray);
            }

            if (top > 0)
            {
                dataArray = dataArray.Take(top).ToArray();
            }

            return dataArray.ToList();
        }
        /// <summary>
        /// 根据字段拼接get参数
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string GetPars(Dictionary<string, object> dic)
        {

            StringBuilder sb = new StringBuilder();
            string urlPars = null;
            bool isEnter = false;
            foreach (var item in dic)
            {
                sb.Append($"{(isEnter ? "&" : "")}{item.Key}={item.Value}");
                isEnter = true;
            }
            urlPars = sb.ToString();
            return urlPars;
        }
        /// <summary>
        /// 根据字段拼接get参数
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string GetPars(Dictionary<string, string> dic)
        {

            StringBuilder sb = new StringBuilder();
            string urlPars = null;
            bool isEnter = false;
            foreach (var item in dic)
            {
                sb.Append($"{(isEnter ? "&" : "")}{item.Key}={item.Value}");
                isEnter = true;
            }
            urlPars = sb.ToString();
            return urlPars;
        }
        /// <summary>
        /// 获取一个GUID
        /// </summary>
        /// <param name="format">格式-默认为N</param>
        /// <returns></returns>
        public static string GetGUID(string format = "N")
        {
            return Guid.NewGuid().ToString(format);
        }
        /// <summary>  
        /// 根据GUID获取19位的唯一数字序列  
        /// </summary>  
        /// <returns></returns>  
        public static long GetGuidToLongID()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }
        /// <summary>
        /// 获取字符串最后X行
        /// </summary>
        /// <param name="resourceStr"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetCusLine(string resourceStr, int length)
        {
            string[] arrStr = resourceStr.Split("\r\n");
            return string.Join("", (from q in arrStr select q).Skip(arrStr.Length - length + 1).Take(length).ToArray());
        }
        /// <summary>
        /// 下划线转驼峰
        /// </summary>
        /// <param name="snakeCaseString"></param>
        /// <returns></returns>
        public static string ToCamelCase(string snakeCaseString)
        {
            if (string.IsNullOrEmpty(snakeCaseString))
            {
                return snakeCaseString;
            }

            var parts = snakeCaseString.Split('_');
            for (int i = 1; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
                }
            }

            return string.Concat(parts);
        }
        /// <summary>
        /// 驼峰转下划线
        /// </summary>
        /// <param name="camelCaseString"></param>
        /// <returns></returns>
        public static string ToSnakeCase(string camelCaseString)
        {
            if (string.IsNullOrEmpty(camelCaseString))
            {
                return camelCaseString;
            }

            var stringBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < camelCaseString.Length; i++)
            {
                if (char.IsUpper(camelCaseString[i]))
                {
                    if (i > 0)
                    {
                        stringBuilder.Append('_');
                    }
                    stringBuilder.Append(char.ToLower(camelCaseString[i]));
                }
                else
                {
                    stringBuilder.Append(camelCaseString[i]);
                }
            }

            return stringBuilder.ToString();
        }
        /// <summary>
        ///  下划线转驼峰(首字母大写-Pascal Case)
        /// </summary>
        /// <param name="snakeCaseString"></param>
        /// <returns></returns>
        public static string ToPascalCase(string snakeCaseString)
        {
            if (string.IsNullOrEmpty(snakeCaseString))
            {
                return snakeCaseString;
            }

            var parts = snakeCaseString.Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
                }
            }

            return string.Concat(parts);
        }
        /// <summary>
        /// 随机获取中文
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateRandomChinese(int length)
        {
            Random random = new Random();
            StringBuilder sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int unicodeValue = random.Next(0x4E00, 0x9FFF + 1);
                char chineseChar = (char)unicodeValue;
                sb.Append(chineseChar);
            }

            return sb.ToString();
        }
        /// <summary>
        /// 字符串转base64
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringToBase64(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }
        /// <summary>
        /// base64转字符串
        /// </summary>
        /// <param name="base64str"></param>
        /// <returns></returns>
        public static string Base64ToString(string base64str)
        {
            return Encoding.UTF8.GetString( Convert.FromBase64String(base64str));
        }
    }
}

