using MyDotnet.Domain.Dto.Guiji;
using MyDotnet.Domain.Dto.Sannuo;
using SixLabors.ImageSharp.ColorSpaces;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace MyDotnet.Helper
{
    /// <summary>
    /// 三诺帮助类
    /// </summary>
    public static class SannuoHelper
    {
        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <returns></returns>
        public static async Task<SannuoSmsReturnDto> sendSms(string phone)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://ican.sinocare.com/api/sino-notice/v1/message/send-validate-sms");

            SannuoSmsDto sannuoSmsDto = new SannuoSmsDto();
            sannuoSmsDto.phone = phone;
            sannuoSmsDto.nonce = phone;
            request.Content = new StringContent(JsonHelper.ObjToJson(sannuoSmsDto,false), Encoding.UTF8, "application/json");
            request.Headers.Add("Authorization", "Basic c2luby1pY2FuLWlvczphNDM5ZTY1ODRmOTg0ZGNiODFhMzliOGQ5NjUxNjEwNA==");
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<SannuoSmsReturnDto>(res);
            return data;
        }
        /// <summary>
        /// 登录(登录成功没有成功标记,只有access_token字段是否有值)
        /// </summary>
        /// <returns></returns>
        public static async Task<SannuoSmsLoginReturnDto> login(string phone,string code)
        { 
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(code), "code");
            formData.Add(new StringContent("sms"), "grant_type");
            formData.Add(new StringContent("2409:8920:c231:d20d:2c88:d381:2e91:d356"), "ipv6Addr");
            formData.Add(new StringContent(phone), "phone");

            // 创建HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://ican.sinocare.com/api/sino-auth/oauth/token");
            request.Content = formData; 
            // 设置请求头
            request.Headers.Add("Authorization", "Basic c2luby1pY2FuLWlvczphNDM5ZTY1ODRmOTg0ZGNiODFhMzliOGQ5NjUxNjEwNA==");

            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<SannuoSmsLoginReturnDto>(res);
            return data;
        }
        /// <summary>
        /// 获取token信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<SannuoMyInfoDto> getMyInfo(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://ican.sinocare.com/api/sino-archives/v1/user/info");
            request.Headers.Add("Sino-Auth", token);
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<SannuoMyInfoDto>(res);
            return data;
        }
        /// <summary>
        /// 获取监护者列表
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<SannuoFamilyDto> getFamily(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://ican.sinocare.com/api/sino-archives/archivesUserFamily/family");
            request.Headers.Add("Sino-Auth", token);
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<SannuoFamilyDto>(res);
            return data;
        }
        /// <summary>
        /// 获取监护者详情
        /// </summary>
        /// <param name="token"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async Task<SannuoFamilyUserDto> getFamilyUserInfo(string token,string uid)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://ican.sinocare.com/api/sino-archives/archivesUserFamilySetting/info?userId={uid}");
            request.Headers.Add("Sino-Auth", token);
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<SannuoFamilyUserDto>(res);
            return data;
        }
        /// <summary>
        /// 获取某个监护者血糖
        /// </summary>
        /// <param name="token"></param>
        /// <param name="suid">加密id</param>
        /// <returns></returns>
        public static async Task<SannuoBloodDto> getUserBlood(string token, string suid)
        {
            DateTimeOffset start = DateTimeOffset.Now.Date;
            DateTimeOffset end = DateTimeOffset.Now.Date.AddDays(1);
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://ican.sinocare.com/api/sino-health/v1/cgm-data/list?sortType=1&minTimestamp={start.ToUnixTimeMilliseconds()}&maxTimestamp={end.ToUnixTimeMilliseconds()}&familyUserId={WebUtility.UrlEncode(suid)}");
            request.Headers.Add("Sino-Auth", token);
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<SannuoBloodDto>(res);

            foreach (var item in data.data)
            {
                item.parsTime = DateTimeOffset.FromUnixTimeMilliseconds(item.time).UtcDateTime.ToLocalTime();
            }
            return data;
        }
    }
}
