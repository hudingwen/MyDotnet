using Microsoft.Extensions.DependencyInjection;
using Quartz;
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
        private static HttpClient httpClient { get
            {
                using (var scope = AppHelper.appService.CreateScope())
                {
                    var data = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                    return data.CreateClient();
                }
            }
        }
        /// <summary>
        /// 发送get请求
        /// </summary>
        /// <param name="serviceAddress"></param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string serviceAddress)
        {
            return await httpClient.GetStringAsync(serviceAddress);
        }
        /// <summary>
        /// 发送get请求
        /// </summary>
        /// <param name="serviceAddress"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> GetAsyncResponse(string serviceAddress)
        {
            return await httpClient.GetAsync(serviceAddress); 
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
                using (var response = await httpClient.PostAsync(serviceAddress, httpContent))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
        public static async Task<string> PostAsync(string serviceAddress, HttpContent httpContent)
        {
            try
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using (var response = await httpClient.PostAsync(serviceAddress, httpContent))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                httpContent.Dispose();
            }
        }
        public static async Task<string> SendAsync(HttpRequestMessage request,Dictionary<string,string> responseHeaderDic=null)
        {
            try
            {
                using (var response = await httpClient.SendAsync(request))
                {
                    var status = response.EnsureSuccessStatusCode();
                    var data = await response.Content.ReadAsStringAsync();
                    if(responseHeaderDic != null)
                    {
                        //提取responseHeader所需内容
                        foreach (var key in responseHeaderDic.Keys)
                        {
                            IEnumerable<string>? values;
                            if(response.Headers.TryGetValues(key, out values))
                            {
                                responseHeaderDic[key] = string.Join("",values);
                            } 
                        }
                    }
                    return data;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                request.Dispose();
            }
        }
    }


}
