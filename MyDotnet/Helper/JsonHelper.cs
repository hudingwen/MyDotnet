using Microsoft.Extensions.Options;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Services.System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MyDotnet.Helper
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings;
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static JsonHelper()
        {
            jsonSerializerSettings = new JsonSerializerSettings();
            //忽略循环引用
            jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            //不使用驼峰样式的key
            jsonSerializerSettings.ContractResolver = new DefaultContractResolver();
            //设置时间格式
            jsonSerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            //忽略为null的字段
            //jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            //设置本地时间而非UTC时间
            jsonSerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            //设置枚举首字符小写
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            //将long类型转为string
            jsonSerializerSettings.Converters.Add(new LongToStringConverter());
            //是否美化
            jsonSerializerSettings.Formatting = Formatting.Indented;
        }
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="obj">对象</param> 
        /// <returns>返回json字符串</returns>
        public static string ObjToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, jsonSerializerSettings);
        }
        /// <summary>
        /// json反序列化obj
        /// </summary>
        /// <typeparam name="T">反序列类型</typeparam>
        /// <param name="strJson">json</param>
        /// <returns>返回对象</returns>
        public static T JsonToObj<T>(string strJson)
        {
            return JsonConvert.DeserializeObject<T>(strJson, jsonSerializerSettings);
        }
    }
}
