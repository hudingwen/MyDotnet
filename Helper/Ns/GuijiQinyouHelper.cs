using MyDotnet.Domain.Dto.Guiji;
using MyDotnet.Domain.Dto.GuijiLite;
using System.Text;

namespace MyDotnet.Helper.Ns
{
    /// <summary>
    /// 硅基亲友帮助类
    /// </summary>
    public static class GuijiQinyouHelper
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static async Task<GuijiQinyouLoginReturnDto> loginGuiji(string name,string pass)
        {
            GuiQinyouLoginDto loginDto = new GuiQinyouLoginDto();
            loginDto.phone = name;
            loginDto.password = MD5Helper.MD5Encrypt32(pass,true);
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://cxm-api.sisensing.com/cxm-app-sijoy-health/app/identity/password/login");
            request.Content = new StringContent(JsonHelper.ObjToJson(loginDto), Encoding.UTF8, "application/json");
            request.Headers.Add("Sib-Agent", "SIJOY_HEALTH_APP&01.02.00.00&iOS&18.7.3&iPhone17,1");
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<GuijiQinyouLoginReturnDto>(res);
            return data;
        }
        /// <summary>
        /// 获取token信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<GuijiQinyouMyInfo> getMyInfo(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://cxm-api.sisensing.com/cxm-mini-program-share/user/info");
            request.Headers.Add("Authorization", $"Bearer {token}");
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<GuijiQinyouMyInfo>(res);
            return data;
        }
        /// <summary>
        /// 获取监护者列表
        /// </summary>
        /// <param name="token"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static async Task<GuijiQinyouFollowDto> getGuijiList(string token, int page = 1, int size = 10)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://cxm-api.sisensing.com/cxm-mini-program-share/follow/relation/followDeviceList?pageNum={page}&pageSize={size}");
            request.Headers.Add("Authorization", $"Bearer {token}");
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<GuijiQinyouFollowDto>(res);

            return data;
        }
        /// <summary>
        /// 获取某个监护者
        /// </summary>
        /// <param name="token"></param>
        /// <param name="uid"></param>
        /// <param name="did">设备id</param>
        /// <returns></returns>
        public static async Task<GuijiQinyouUserBloodDto> getUserBlood(string token, string uid,string did)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://cxm-api.sisensing.com/cxm-mini-program-share/follow/relation/data/details?appCode=SIJOY_HEALTH_APP&deviceId={did}&followRelationId={uid}");
            request.Headers.Add("Authorization", $"Bearer {token}");
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<GuijiQinyouUserBloodDto>(res);


            data.data.deviceInfo.latestTimeFormat = DateTimeOffset.FromUnixTimeMilliseconds(data.data.deviceInfo.latestTime.ObjToLong()).UtcDateTime.ToLocalTime();
            data.data.deviceInfo.latestValueFormat = data.data.deviceInfo.latestValue.ObjToMoney();

            foreach (var item in data.data.deviceInfo.glucoseInfos)
            {
                item.time = DateTimeOffset.FromUnixTimeMilliseconds(item.t).UtcDateTime.ToLocalTime();
                item.blood = item.v.ObjToMoney();
            }
            data.data.deviceInfo.glucoseInfos = data.data.deviceInfo.glucoseInfos.OrderByDescending(t => t.time).ToList();
            return data;
        }
    }
}
