using Newtonsoft.Json;

namespace MyDotnet.Helper
{
    public static class JsonHelper
    {
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>返回json字符串</returns>
        public static string ObjToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        /// <summary>
        /// json反序列化obj
        /// </summary>
        /// <typeparam name="T">反序列类型</typeparam>
        /// <param name="strJson">json</param>
        /// <returns>返回对象</returns>
        public static T JsonToObj<T>(string strJson)
        {
            return JsonConvert.DeserializeObject<T>(strJson);
        }
    }
}
