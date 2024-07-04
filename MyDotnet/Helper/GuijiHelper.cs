using MyDotnet.Domain.Dto.Guiji;
using System.Text;

namespace MyDotnet.Helper
{
    /// <summary>
    /// 硅基帮助类
    /// </summary>
    public static class GuijiHelper
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static async Task<GuiLoginReturnDto> loginGuiji(string phone,string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.sisensing.com/auth/app/user/login");

            GuiLoginDto loginDto = new GuiLoginDto();
            loginDto.phone = phone;
            loginDto.password = password;
            loginDto.device_number = "9C2C740F95EF45709ABAAE62145CF2C6";
            loginDto.loginType = "2";
            loginDto.device_type = "iOS";
            request.Content = new StringContent(JsonHelper.ObjToJson(loginDto), Encoding.UTF8, "application/json");
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<GuiLoginReturnDto>(res);
            return data;
        }
        /// <summary>
        /// 获取token信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<GuiMyInfoDto> getMyInfo(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.sisensing.com/auth/token/info");
            request.Headers.Add("Authorization", token);
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<GuiMyInfoDto>(res);
            return data;
        }
        /// <summary>
        /// 获取监护者列表
        /// </summary>
        /// <param name="token"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static async Task<GuiFollowDto> getGuijiList(string token, int page = 1, int size = 10)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.sisensing.com/follow/app/list/v2?pageNum={page}&pageSize={size}&status=3");
            request.Headers.Add("Authorization", token);
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<GuiFollowDto>(res);
            return data;
        }
        /// <summary>
        /// 获取某个监护者
        /// </summary>
        /// <param name="token"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async Task<GuiBloodDto> getUserBlood(string token, string uid)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.sisensing.com/follow/app/{uid}/v2");
            request.Headers.Add("Authorization", token);
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<GuiBloodDto>(res);


            data.data.followedDeviceGlucoseDataPO.time = DateTimeOffset.FromUnixTimeMilliseconds(data.data.followedDeviceGlucoseDataPO.latestGlucoseTime).UtcDateTime.ToLocalTime();

            foreach (var item in data.data.followedDeviceGlucoseDataPO.glucoseInfos)
            {
                item.time = DateTimeOffset.FromUnixTimeMilliseconds(item.t).UtcDateTime.ToLocalTime();
            }
            data.data.followedDeviceGlucoseDataPO.glucoseInfos = data.data.followedDeviceGlucoseDataPO.glucoseInfos.OrderByDescending(t => t.time).ToList();
            return data;
        }
    }
}
