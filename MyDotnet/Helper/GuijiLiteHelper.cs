using MyDotnet.Domain.Dto.Guiji;
using MyDotnet.Domain.Dto.GuijiLite;
using System.Text;

namespace MyDotnet.Helper
{
    /// <summary>
    /// 硅基轻享帮助类
    /// </summary>
    public static class GuijiLiteHelper
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static async Task<GuijiLiteLoginReturnDto> loginGuiji(string phone,string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.sisensing.com/lite-sense-app/user/password/login");

            GuijiLiteLoginDto loginDto = new GuijiLiteLoginDto();
            loginDto.phone = phone;
            loginDto.password = password;
            request.Content = new StringContent(JsonHelper.ObjToJson(loginDto), Encoding.UTF8, "application/json");
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<GuijiLiteLoginReturnDto>(res);
            return data;
        }
        /// <summary>
        /// 获取token信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<GuijiLiteMyInfo> getMyInfo(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.sisensing.com/lite-sense-app/user/info");
            request.Headers.Add("Authorization", $"Bearer {token}");
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<GuijiLiteMyInfo>(res);
            return data;
        }
        /// <summary>
        /// 获取监护者列表
        /// </summary>
        /// <param name="token"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static async Task<GuijiLiteFollowDto> getGuijiList(string token, int page = 1, int size = 10)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.sisensing.com/lite-sense-app/follow/list?pageNum={page}&pageSize={size}&status=3");
            request.Headers.Add("Authorization", $"Bearer {token}");
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<GuijiLiteFollowDto>(res);
            return data;
        }
        /// <summary>
        /// 获取某个监护者
        /// </summary>
        /// <param name="token"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async Task<GuijiLiteUserBloodDto> getUserBlood(string token, string uid)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.sisensing.com/lite-sense-app/follow/info?followId={uid}");
            request.Headers.Add("Authorization", $"Bearer {token}");
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<GuijiLiteUserBloodDto>(res);


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
