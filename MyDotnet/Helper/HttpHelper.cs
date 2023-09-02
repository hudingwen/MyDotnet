using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MyDotnet.Helper
{
    /// <summary>
    /// httpclinet请求方式，请尽量使用IHttpClientFactory方式
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// 发送get请求
        /// </summary>
        /// <param name="serviceAddress"></param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string serviceAddress)
        {
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetStringAsync(serviceAddress);
            }
        }
        /// <summary>
        /// 发送post请求
        /// </summary>
        /// <param name="serviceAddress"></param>
        /// <param name="requestJson"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string serviceAddress, string requestJson = null)
        {
            using (HttpContent httpContent = new StringContent(requestJson))
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.PostAsync(serviceAddress, httpContent))
                    {
                        return await response.Content.ReadAsStringAsync();
                    }

                }
            }
        }
    }


}
