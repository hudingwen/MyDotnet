using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.Apple;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services.System;

namespace MyDotnet.Controllers.Ns
{

    /// <summary>
    /// 苹果api
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class AppleController : BaseApiController
    {
        public DicService _dictService;
        public AppleController(DicService dictService)
        {
            _dictService = dictService;
        }
        /// <summary>
        /// 获取苹果api列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<DicData>>> GetAppleApiDicList()
        {
            var data = await _dictService.GetDicData(DicAppleInfo.AppleApiList);
            return MessageModel<List<DicData>>.Success("获取成功", data.Select(t => new DicData { code = t.code, name = t.name }).ToList());
        }
        /// <summary>
        /// 获取描述文件列表
        /// </summary>
        /// <param name="kid"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<ProfilesListDto>> GetProfiles(string kid, int page = 1, int size = 10)
        {
            var data = await _dictService.GetDicDataOne(DicAppleInfo.AppleApiList, kid);
            var token = AppleHelper.GetNewAppleToken(data.code, data.content, data.content2);
            var list = await AppleHelper.GetProfiles(token, page, size);
            return MessageModel<ProfilesListDto>.Success("获取成功", list);
        }
        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <param name="kid"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<DevicesListDto>> GetDevices(string kid, int page = 1, int size = 10)
        {
            var data = await _dictService.GetDicDataOne(DicAppleInfo.AppleApiList, kid);
            var token = AppleHelper.GetNewAppleToken(data.code, data.content, data.content2);
            var list = await AppleHelper.GetDevices(token, page, size);
            return MessageModel<DevicesListDto>.Success("获取成功", list);
        }
        /// <summary>
        /// 为设备生成一个描述文件
        /// </summary>
        /// <param name="kid">证书id</param>
        /// <param name="name">设备名称</param>
        /// <param name="udid">设备udid</param>
        /// <param name="profileId">源配置id</param>
        /// <param name="profileName">配置名称(新增用)</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<ProfilesReturnAdd>> AddProfileForDevice(string kid, string name, string udid,string profileId="", string profileName = "")
        {
            name = name.Trim();
            udid = udid.Trim();
            var data = await _dictService.GetDicDataOne(DicAppleInfo.AppleApiList, kid);
            var token = AppleHelper.GetNewAppleToken(data.code, data.content, data.content2);
            AppleCerInfo appleCerInfo = JsonHelper.JsonToObj<AppleCerInfo>(data.content3);

            ProfilesReturnAdd info = null;
            if (!string.IsNullOrEmpty(profileId))
            {
                //追加配置
                info = await AppleHelper.GetProfile(token, profileId);
                var findDevic =  info.data.relationships.devices.data.Find(t => t.attributes.udid.Equals(udid));
                if(findDevic != null)
                {
                   return MessageModel<ProfilesReturnAdd>.Fail("设别udid已经添加过了,无需重复添加");
                }
            }




            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //设备
            var findDevices = await AppleHelper.GetDevices(token,1,1, udid);

            string deviceId = "";
            if(findDevices.data == null | findDevices.data.Count == 0)
            {
                //新增设备
                var devicesAdd = new DevicesAdd();
                devicesAdd.data.type = "devices";
                devicesAdd.data.attributes.name = $"自动配置:{time}|{name}";
                devicesAdd.data.attributes.platform = "IOS";
                devicesAdd.data.attributes.udid = udid;
                var device = await AppleHelper.AddDevices(token, devicesAdd);
                deviceId = device.data.id;
            }
            else
            {
                deviceId = findDevices.data[0].id;
            }
            

            //配置
            ProfilesAdd profilesAdd = new ProfilesAdd();
            profilesAdd.data.type = "profiles";
            if (info == null)
            {
                //新增配置
                profilesAdd.data.attributes.name = $"自动配置:{time}|{profileName}";
            }
            else
            {
                //追加配置
                var spName = info.data.attributes.name.Split('|');
                profilesAdd.data.attributes.name = $"自动配置:{time}|{(spName.Length > 0 ? spName[spName.Length - 1] : (string.IsNullOrEmpty(profileName) ? name : profileName))}";

            }
            profilesAdd.data.attributes.profileType = "IOS_APP_DEVELOPMENT";


            profilesAdd.data.relationships.bundleId.data.type = "bundleIds";
            profilesAdd.data.relationships.bundleId.data.id = appleCerInfo.bundleIds;


            profilesAdd.data.relationships.certificates.data.Add(new ProfilesAddDataRelationshipsCertificatesData()
            {
                type = "certificates",
                id = appleCerInfo.certificates
            });



            profilesAdd.data.relationships.devices.data.Add(new ProfilesAddDataRelationshipsDevicesData()
            {
                type = "devices",
                id = deviceId
            });

            if (info == null)
            {
                //新增配置
            }
            else
            {
                //追加配置
                //添加原来的设备进来
                foreach (var item in info.data.relationships.devices.data)
                {
                    profilesAdd.data.relationships.devices.data.Add(new ProfilesAddDataRelationshipsDevicesData()
                    {
                        type = "devices",
                        id = item.id
                    });
                }


            }

            var profile = await AppleHelper.AddProfiles(token, profilesAdd);

            if (info == null)
            {
                //新增配置
            }
            else
            {
                //追加配置
                //追加后删除原来的
                await AppleHelper.DelProfile(token, profileId);

            }
            return MessageModel<ProfilesReturnAdd>.Success("获取成功", profile);

        }

        /// <summary>
        /// 下载描述文件
        /// </summary>
        /// <param name="kid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> DownloadProfile(string kid, string id)
        {
            var data = await _dictService.GetDicDataOne(DicAppleInfo.AppleApiList, kid);
            var token = AppleHelper.GetNewAppleToken(data.code, data.content, data.content2);
            var profile = await AppleHelper.GetProfile(token, id); 

            byte[] fileBytes = Convert.FromBase64String(profile.data.attributes.profileContent);
            var fileStream = new MemoryStream(fileBytes);
            string fileName = $"{profile.data.id}.mobileprovision";
            return File(fileStream, "application/octet-stream", fileName);
        }

        /// <summary>
        /// 删除描述文件
        /// </summary>
        /// <param name="kid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<string>> DelProfile(string kid, string id)
        {
            var data = await _dictService.GetDicDataOne(DicAppleInfo.AppleApiList, kid);
            var token = AppleHelper.GetNewAppleToken(data.code, data.content, data.content2);
            var profile = await AppleHelper.DelProfile(token, id);
            return MessageModel<string>.Success("删除成功",profile);
        }
    }

}
