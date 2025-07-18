using System.Security.Cryptography;
using System.Text;
using JWT.Algorithms;
using JWT.Builder;
using MyDotnet.Domain.Dto.Apple;

namespace MyDotnet.Helper.Ns
{


    /// <summary>
    /// 苹果开发者帮助类
    /// </summary>
    public static class AppleHelper
    {
        /// <summary>
        /// 苹果api地址
        /// </summary>
        public static string baseUrl = "https://api.appstoreconnect.apple.com";

        /// <summary>
        /// 生成苹果api访问toekn
        /// </summary>
        /// <returns></returns>
        public static string GetNewAppleToken(string kid, string securityKey, string issuerId)
        {
            securityKey = securityKey.Replace("-----BEGIN PRIVATE KEY-----", "").Trim();
            securityKey = securityKey.Replace("-----END PRIVATE KEY-----", "").Trim();
            securityKey = securityKey.Replace("\r\n", "").Trim();
            securityKey = securityKey.Replace("\n", "").Trim();

            var pk = ECDsa.Create();
            pk.ImportPkcs8PrivateKey(Convert.FromBase64String(securityKey), out var bytesRead);

            var now = DateTimeOffset.UtcNow;
            var exp = now.AddMinutes(19);

            var jwtBuilder = new JwtBuilder()
                .WithAlgorithm(new ES256Algorithm(ECDsa.Create(), pk))
                .AddHeader("kid", kid);

            var payload = new Dictionary<string, object>
            {
                { "iss", issuerId },
                { "iat", now.ToUnixTimeSeconds() },
                { "exp", exp.ToUnixTimeSeconds() },
                { "aud", "appstoreconnect-v1" }
            };
            return jwtBuilder.Encode(payload);
        }
        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <param name="token"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="status">状态 PROCESSING-审核中 ENABLED-可用 DISABLED-禁用</param>
        /// <param name="udid"></param>
        /// <returns></returns>
        public static async Task<DevicesListDto> GetDevices(string token, int page = 1, int size = 10,string status="", string udid = "")
        {
            var offset = $"{{\"offset\":\"{(page - 1) * size}\"}}";
            string base64Str = StringHelper.StringToBase64(offset);
            //filter[name]=自动配置&
            var url = baseUrl + $"/v1/devices?sort=-name{(string.IsNullOrEmpty(status) ? "" : "&filter[status]=" + status)}{(string.IsNullOrEmpty(udid) ? "" : "&filter[udid]=" + udid)}&limit={size}{(page == 1 ? "" : "&cursor=" + base64Str)}";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequestMessage.Headers.Add("Authorization", "Bearer " + token);
            httpRequestMessage.Headers.Add("Accept", "application/json");
            //httpRequestMessage.Headers.Add("Content-Type", "application/json");
            //httpRequestMessage.Headers.Add("User-Agent", WebUtility.UrlEncode("OpenAPI-Generator/1.0.0/csharp"));

            var json = await HttpHelper.SendAsync(httpRequestMessage);

            var devices = JsonHelper.JsonToObj<DevicesListDto>(json);
            return devices;
        }
        /// <summary>
        /// 获取配置列表(描述文件)
        /// </summary>
        /// <param name="token"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static async Task<ProfilesListDto> GetProfiles(string token,int page=1,int size=10)
        {
            //{"offset":"2"}
            var offset = $"{{\"offset\":\"{(page - 1) * size}\"}}";
            string base64Str = StringHelper.StringToBase64(offset);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/v1/profiles?filter[name]=自动配置&sort=-name&limit={size}{(page==1?"": "&cursor="+ base64Str)}");
            httpRequestMessage.Headers.Add("Authorization", "Bearer " + token);
            httpRequestMessage.Headers.Add("Accept", "application/json");
            //httpRequestMessage.Headers.Add("Content-Type", "application/json");
            //httpRequestMessage.Headers.Add("User-Agent", WebUtility.UrlEncode("OpenAPI-Generator/1.0.0/csharp"));
            var json = await HttpHelper.SendAsync(httpRequestMessage);
            //LogHelper.logApp.Info(json);
            var profiles = JsonHelper.JsonToObj<ProfilesListDto>(json);
            foreach (var profile in profiles.data)
            {

                if (profile.relationships.devices.data == null) profile.relationships.devices.data = new List<ProfilesReturnAddDataRelationshipsDevicesData>();

                //获取设备列表
                var devicesStr = await GetUrlData(token, profile.relationships.devices.links.related + "?limit=200");
                var devices =  JsonHelper.JsonToObj<ProfilesReturnAddDataRelationshipsDevices>(devicesStr);
                profile.relationships.devices = devices;
                ////获取配置设备列表
                //var httpRequestProfile = new HttpRequestMessage(HttpMethod.Get, profile.relationships.devices.links.self);
                //httpRequestProfile.Headers.Add("Authorization", "Bearer " + token);
                //httpRequestProfile.Headers.Add("Accept", "application/json");
                //var devices = JsonHelper.JsonToObj<ProfilesAddDataRelationshipsDevices>(await HttpHelper.SendAsync(httpRequestProfile));
                //foreach (var device in devices.data)
                //{
                //    //获取设备详情 
                //    var httpRequestDevice = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/v1/devices/{device.id}");
                //    httpRequestDevice.Headers.Add("Authorization", "Bearer " + token);
                //    httpRequestDevice.Headers.Add("Accept", "application/json");
                //    var deviceInfo = JsonHelper.JsonToObj<DevicesReturnAdd>(await HttpHelper.SendAsync(httpRequestDevice));
                //    profile.relationships.devices.data.Add(new ProfilesReturnAddDataRelationshipsDevicesData { id = deviceInfo.data.id, type = deviceInfo.data.type, deviceInfo = deviceInfo.data.attributes });
                //}

            }
            return profiles;
        } 
        /// <summary>
        /// 添加一个描述文件
        /// </summary>
        /// <param name="token"></param>
        /// <param name="profilesAdd"></param>
        /// <returns></returns>
        internal static async Task<ProfilesReturnAdd> AddProfiles(string token, ProfilesAdd profilesAdd)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/v1/profiles");
            httpRequestMessage.Headers.Add("Authorization", "Bearer " + token);
            httpRequestMessage.Headers.Add("Accept", "application/json");
            //httpRequestMessage.Headers.Add("Content-Type", "application/json");
            //httpRequestMessage.Headers.Add("User-Agent", WebUtility.UrlEncode("OpenAPI-Generator/1.0.0/csharp"));
           
            var content = new StringContent(JsonHelper.ObjToJson(profilesAdd), Encoding.UTF8, "application/json");
            JsonContent jsonContent = JsonContent.Create(profilesAdd, typeof(ProfilesAdd));
            httpRequestMessage.Content = jsonContent;
            return JsonHelper.JsonToObj<ProfilesReturnAdd>(await HttpHelper.SendAsync(httpRequestMessage));
        } 
        /// <summary>
        /// 添加一个设备
        /// </summary>
        /// <param name="token"></param>
        /// <param name="devicesAdd"></param>
        /// <returns></returns>

        internal static async Task<DevicesReturnAdd> AddDevices(string token, DevicesAdd devicesAdd)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/v1/devices");
            httpRequestMessage.Headers.Add("Authorization", "Bearer " + token);
            httpRequestMessage.Headers.Add("Accept", "application/json");
            var content = new StringContent(JsonHelper.ObjToJson(devicesAdd), Encoding.UTF8, "application/json");
            httpRequestMessage.Content = content;
            return JsonHelper.JsonToObj<DevicesReturnAdd>(await HttpHelper.SendAsync(httpRequestMessage));
        }
        /// <summary>
        /// 获取描述文件详情
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static async Task<ProfilesReturnAdd> GetProfile(string token, string id)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, baseUrl + $"/v1/profiles/{id}");
            httpRequestMessage.Headers.Add("Authorization", "Bearer " + token);
            httpRequestMessage.Headers.Add("Accept", "application/json");
            var data = await HttpHelper.SendAsync(httpRequestMessage);
            var profile = JsonHelper.JsonToObj<ProfilesReturnAdd>(data);
            //获取设备列表
            var devicesStr = await GetUrlData(token, profile.data.relationships.devices.links.related + "?limit=200");
            var devices = JsonHelper.JsonToObj<ProfilesReturnAddDataRelationshipsDevices>(devicesStr);
            profile.data.relationships.devices = devices;

            return profile;
        }
        /// <summary>
        /// 删除一个描述文件
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static async Task<string> DelProfile(string token, string id)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, baseUrl + $"/v1/profiles/{id}");
            httpRequestMessage.Headers.Add("Authorization", "Bearer " + token);
            httpRequestMessage.Headers.Add("Accept", "application/json");
            return await HttpHelper.SendAsync(httpRequestMessage);
        }

        internal static async Task<string> GetUrlData(string token, string url)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequestMessage.Headers.Add("Authorization", "Bearer " + token);
            httpRequestMessage.Headers.Add("Accept", "application/json");
            return await HttpHelper.SendAsync(httpRequestMessage);
        }
    }
}

