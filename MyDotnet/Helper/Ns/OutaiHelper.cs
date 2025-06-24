using Dm.filter.log;
using MyDotnet.Domain.Dto.Guiji;
using MyDotnet.Domain.Dto.Outai;
using MyDotnet.Domain.Dto.Weitai1;
using MyDotnet.Domain.Dto.Weitai2;
using NetTaste;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace MyDotnet.Helper.Ns
{
    /// <summary>
    /// 欧泰帮助类
    /// </summary>
    public static class OutaiHelper
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public static async Task<OutaiLoginReturnDto> Login(string phone, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.365healthy.net/fitness/auth/login");

            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("phone", phone),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("userType", "2"),
            });
            var res = await HttpHelper.SendAsync(request);

            var data = JsonHelper.JsonToObj<OutaiLoginReturnDto>(res);
            if (data.state == 1)
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(data.token) as JwtSecurityToken;

                if (jsonToken != null)
                {
                    var exp = jsonToken.Claims.First(claim => claim.Type == "exp").Value;
                    var expDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime.ToLocalTime();
                    data.tokenExpire = expDateTime;
                }
            }

            return data;
        }
        /// <summary>
        /// 获取token信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static async Task<OutaiMyInfoDto> getMyInfo(string token, string phone)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.365healthy.net/userMessage/getBasicInformationVo");

            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("phone", phone), 
                new KeyValuePair<string, string>("token", token),
            });
            var res = await HttpHelper.SendAsync(request);

            var data = JsonHelper.JsonToObj<OutaiMyInfoDto>(res);
            return data;
        }
        /// <summary>
        /// 获取监护者列表
        /// </summary>
        /// <param name="token"></param>
        /// <param name="phone"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<OutaiFamilyDto> getFamily(string token,string phone, string page = "1", string pageSize = "9999")
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.365healthy.net/app/user/correlation/get/associate/friend");

            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("phone", phone),
                new KeyValuePair<string, string>("token", token),
                new KeyValuePair<string, string>("page", page),
                new KeyValuePair<string, string>("pageSize", pageSize),
            });
            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<OutaiFamilyDto>(res);

            if(data.state == 1)
            {
                foreach (var item in data.associateFriendList)
                {
                    var requestPhone = new HttpRequestMessage(HttpMethod.Post, $"https://www.365healthy.net/app/user/correlation/friend/basic/message");

                    requestPhone.Content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("phone", phone),
                        new KeyValuePair<string, string>("token", token),
                        new KeyValuePair<string, string>("friendUserId", item.friendUserId)
                    });
                    var resPhone = await HttpHelper.SendAsync(requestPhone);
                    var dataPhone = JsonHelper.JsonToObj<OutaiFamilyPhoneDto>(resPhone);
                    if(dataPhone.state == 1)
                    {
                        item.phone = dataPhone.content.username;
                    }
                }
            }
            return data;
        }
        /// <summary>
        /// 获取某个监护者
        /// </summary>
        /// <param name="token"></param>
        /// <param name="phone"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async Task<OutaiBloodDto> getBlood(string token, string phone,string uid)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.365healthy.net/app/customer/ottai/glucose/realTimeMonitoring?hours={3}&phone={phone}&token={token}&userId={uid}");
           

            var res = await HttpHelper.SendAsync(request);
            var data = JsonHelper.JsonToObj<OutaiBloodDto>(res);
            if (data.state == 1 && data.content != null && data.content.bloodSugarRecords != null && data.content.bloodSugarRecords.records != null)
            {
                for (int i = data.content.bloodSugarRecords.records.Count -1; i>=0; i--)
                {
                    var curRow = data.content.bloodSugarRecords.records[i];
                    if (i > 0) {
                        //判断前面有数据
                        var preRow = data.content.bloodSugarRecords.records[i - 1];
                        var preSp = preRow.time.Split(":")[0].ObjToInt();
                        var curSp = curRow.time.Split(":")[0].ObjToInt();
                        if(preSp > curSp)
                        {
                            //跨天了 退出循环
                            break;
                        }
                    }
                    curRow.timeFormat = data.content.latestRecordDate.ToString($"yyyy-MM-dd {curRow.time}:ss").ObjToDate();
                    //data.content.bloodSugarRecords.records
                }
            }
            return data;
        }
    }
}
