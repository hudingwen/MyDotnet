using Dm.filter.log;
using MyDotnet.Domain.Dto.Guiji;
using MyDotnet.Domain.Dto.Weitai1;
using MyDotnet.Domain.Dto.Weitai2;
using NetTaste;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace MyDotnet.Helper
{
    /// <summary>
    /// 微泰1帮助类
    /// </summary>
    public static class Weitai2Helper
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static async Task<Weitai2LoginReturnDto> Login(string phone,string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://china.pancares.com/backend/aidex-x/user/loginByPassword");

            Weitai1LoginDto loginDto = new Weitai1LoginDto();
            loginDto.username = phone;
            loginDto.password = MD5Helper.MD5Encrypt32(password,true);
            request.Content = new StringContent(JsonHelper.ObjToJson(loginDto), Encoding.UTF8, "application/json");
             
            var res = await HttpHelper.SendAsync(request);
           
            var data = JsonHelper.JsonToObj<Weitai2LoginReturnDto>(res);
            if (data.code == 200)
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(data.data.token) as JwtSecurityToken;

                if (jsonToken != null)
                {
                    var exp = jsonToken.Claims.First(claim => claim.Type == "exp").Value;
                    var expDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime.ToLocalTime();
                    data.data.tokenExpire = expDateTime;
                }
            }

            return data;
        }
        /// <summary>
        /// 获取token信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<Weitai2MyInfoDto> getMyInfo(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://china.pancares.com/backend/aidex-x/user/getUserInfo");
            request.Headers.Add("token", token);
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<Weitai2MyInfoDto>(res);
            return data;
        }
        /// <summary>
        /// 获取监护者列表
        /// </summary>
        /// <param name="token"></param>
        /// <param name="gid"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static async Task<Weitai2FollowDto> getFamily(string token,string gid, int page = 1, int size = 10)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://china.pancares.com/backend/aidex-x/userAuthorization/findUserAuthorizationList?orderStrategy=ASC&pageNum={page}&pageSize={size}&type=1&userId={gid}");
            request.Headers.Add("token", token);
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<Weitai2FollowDto>(res);
            return data;
        }
        /// <summary>
        /// 获取某个监护者
        /// </summary>
        /// <param name="token"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async Task<Weitai2BloodDto> getBlood(string token, string uid)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://china.pancares.com/backend/aidex-x/cgmRecord/getCgmRecordsByPageInfo?orderStrategy=DESC&pageNum=1&pageSize=300&userId={uid}");
            request.Headers.Add("token", token); 

            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<Weitai2BloodDto>(res);

            return data;
        }
    }
}
