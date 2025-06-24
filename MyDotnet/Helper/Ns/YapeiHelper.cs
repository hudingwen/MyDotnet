using Dm.filter.log;
using MyDotnet.Domain.Dto.Guiji;
using MyDotnet.Domain.Dto.Weitai1;
using MyDotnet.Domain.Dto.Weitai2;
using MyDotnet.Domain.Dto.Yapei;
using NetTaste;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

namespace MyDotnet.Helper.Ns
{
    /// <summary>
    /// 雅培帮助类
    /// </summary>
    public static class YapeiHelper
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static async Task<YapeiLoginReturnInfo> Login(string email, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api-ap.libreview.io/llu/auth/login");

            YapeiLoginInfo loginDto = new YapeiLoginInfo();
            loginDto.email = email;
            loginDto.password = password;
            var sendJson = JsonHelper.ObjToJson(loginDto);
            request.Content = new StringContent(sendJson, Encoding.UTF8, "application/json");

            request.Headers.Remove("User-Agent");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU OS 19_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/19.0 Mobile/10A5355d Safari/8536.25");

            request.Headers.Add("version", "4.14.0");
            request.Headers.Add("product", "llu.ios");

            var res = await HttpHelper.SendAsync(request);
           
            var data = JsonHelper.JsonToObj<YapeiLoginReturnInfo>(res);
            if (data.status == 0)
            {
                data.data.authTicket.tokenExpireTime = DateTimeOffset.FromUnixTimeSeconds(data.data.authTicket.expires).LocalDateTime;
            }

            return data;
        }
        /// <summary>
        /// 获取token信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async Task<YapeiUserReturnInfo> getMyInfo(string token,string uid)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api-ap.libreview.io/llu/connections");

            request.Headers.Remove("User-Agent");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU OS 19_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/19.0 Mobile/10A5355d Safari/8536.25");

            request.Headers.Add("version", "4.14.0");
            request.Headers.Add("product", "llu.ios");

            request.Headers.Add("Authorization", $"Bearer {token}");
            var uidsha = ShaHelper.Sha256(uid);
            request.Headers.Add("Account-Id", uidsha);

            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<YapeiUserReturnInfo>(res);
            return data;
        }
        /// <summary>
        /// 获取监护者列表
        /// </summary>
        /// <param name="token"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async Task<YapeiUserReturnInfo> getFamily(string token, string uid)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api-ap.libreview.io/llu/connections");

            request.Headers.Remove("User-Agent");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU OS 19_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/19.0 Mobile/10A5355d Safari/8536.25");

            request.Headers.Add("version", "4.14.0");
            request.Headers.Add("product", "llu.ios");

            request.Headers.Add("Authorization", $"Bearer {token}");
            var uidsha = ShaHelper.Sha256(uid);
            request.Headers.Add("Account-Id", uidsha);

            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<YapeiUserReturnInfo>(res);
            return data;
        }
        /// <summary>
        /// 获取某个监护者
        /// </summary>
        /// <param name="token"></param>
        /// <param name="uid">监护账号id</param>
        /// <param name="userId">监护用户id</param>
        /// <returns></returns>
        public static async Task<YapeiBloodInfo> getBlood(string token, string uid,string userId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api-ap.libreview.io/llu/connections/{userId}/graph");
            request.Headers.Remove("User-Agent");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU OS 19_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/19.0 Mobile/10A5355d Safari/8536.25");

            request.Headers.Add("version", "4.14.0");
            request.Headers.Add("product", "llu.ios");

            request.Headers.Add("Authorization", $"Bearer {token}");
            var uidsha = ShaHelper.Sha256(uid);
            request.Headers.Add("Account-Id", uidsha);

            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<YapeiBloodInfo>(res);
            if(data.status == 0)
            {
                // 1. 定义格式
                var format = "M/d/yyyy h:mm:ss tt";
                var provider = CultureInfo.InvariantCulture;
                var factoryTimeUtc = DateTime.ParseExact(data.data.connection.glucoseItem.FactoryTimestamp, format, provider, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                

                TimeZoneInfo chinaZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");

                data.data.connection.glucoseItem.time = TimeZoneInfo.ConvertTimeFromUtc(factoryTimeUtc, chinaZone);

                //if(data.data.graphData != null)
                //{
                //    foreach (var item in data.data.graphData)
                //    {
                //        var utcTime = DateTime.ParseExact(item.FactoryTimestamp, format, provider, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

                //        item.time = TimeZoneInfo.ConvertTimeFromUtc(utcTime, chinaZone);
                //    }
                //}
                //else
                //{

                //}
            } 
            return data;
        }
    }
}
