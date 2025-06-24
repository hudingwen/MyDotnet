using Dm.filter.log;
using MyDotnet.Domain.Dto.Guiji;
using MyDotnet.Domain.Dto.Weitai1;
using NetTaste;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace MyDotnet.Helper.Ns
{
    /// <summary>
    /// 微泰1帮助类
    /// </summary>
    public static class Weitai1Helper
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static async Task<Weitai1LoginReturnDto> Login(string phone,string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://china.pancares.com/backend/aidex/api/login?language=zh-Hans-CN");

            Weitai1LoginDto loginDto = new Weitai1LoginDto();
            loginDto.username = phone;
            loginDto.password = MD5Helper.MD5Encrypt32(password,true);
            request.Content = new StringContent(JsonHelper.ObjToJson(loginDto), Encoding.UTF8, "application/json");

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("x-token", "");
            var res = await HttpHelper.SendAsync(request, dic);
           
            var data = JsonHelper.JsonToObj<Weitai1LoginReturnDto>(res);

            if ("100000".Equals(data.info?.code)){
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(dic["x-token"]) as JwtSecurityToken;

                if (jsonToken != null)
                {
                    var exp = jsonToken.Claims.First(claim => claim.Type == "exp").Value;
                    var expDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime.ToLocalTime();

                    data.token = dic["x-token"];
                    data.tokenExpire = expDateTime;
                }
            }
            return data;
        }
        /// <summary>
        /// 获取token信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<Weitai1MyInfoDto> getMyInfo(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://china.pancares.com/backend/aidex/api/user-health-target?language=zh-Hans-CN&currentPage=1&pageSize=1&sortOrder=DESC");
            request.Headers.Add("x-token", token);
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<Weitai1MyInfoDto>(res);
            return data;
        }
        /// <summary>
        /// 获取监护者列表
        /// </summary>
        /// <param name="token"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static async Task<Weitai1FollowDto> getFamily(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://china.pancares.com/backend/aidex/api/follows?language=zh-Hans-CN");
            request.Headers.Add("x-token", token);
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<Weitai1FollowDto>(res);
            return data;
        }
        /// <summary>
        /// 获取某个监护者
        /// </summary>
        /// <param name="token"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async Task<Weitai1BloodDto> getBlood(string token, string uid)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://china.pancares.com/backend/aidex/api/cgm-record/list");
            request.Headers.Add("x-token", token);

            Weitai1SearchBloodDto bloodDto = new Weitai1SearchBloodDto();
            bloodDto.authorizationId = uid;
            bloodDto.pageSize = 300;
            request.Content = new StringContent(JsonHelper.ObjToJson(bloodDto), Encoding.UTF8, "application/json");

            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<Weitai1BloodDto>(res);

            return data;
        }
    }
}
