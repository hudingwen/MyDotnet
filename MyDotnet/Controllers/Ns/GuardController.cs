using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.Apple;
using MyDotnet.Domain.Dto.Guard;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.Sannuo;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services.Ns;
using MyDotnet.Services.System;
using SixLabors.ImageSharp;
using System.Linq.Expressions;

namespace MyDotnet.Controllers.Ns
{

    /// <summary>
    /// 监护api
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class GuardController : BaseApiController
    {
        public DicService _dictService;
        public NightscoutGuardService _guardService;
        private BaseRepository<NightscoutGuardAccount> _baseRepositoryAccount;
        private BaseRepository<Nightscout> _baseRepositoryNightscout;
        public GuardController(DicService dictService
            , NightscoutGuardService guardService
            , BaseRepository<NightscoutGuardAccount> baseRepositoryAccount
            , BaseRepository<Nightscout> baseRepositoryNightscout)
        {
            _dictService = dictService;
            _guardService = guardService;
            _baseRepositoryAccount = baseRepositoryAccount;
            _baseRepositoryNightscout = baseRepositoryNightscout;
        }

        /// <summary>
        /// 刷新监护账户token
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<bool>> refreshGuardAccount(long id)
        {
            var data = await _guardService._baseRepositoryAccount.QueryById(id);
            return await _guardService.refreshGuardAccount(data);
        }
        /// <summary>
        /// 获取监护类型
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<DicData>>> getGuardAccountType()
        {
            var data = await _dictService.GetDicData(GuardInfoKey.GuardAccountTypeList);
            return MessageModel<List<DicData>>.Success("获取成功", data);
        }

        /// <summary>
        /// 发送三诺验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> sendSannuoSms(string phone)
        {
            var res = await SannuoHelper.sendSms(phone);
            if (res.success)
            {
                return MessageModel<string>.Success("发送成功");
            }
            else
            {
                return MessageModel<string>.Fail("发送失败", res.msg);
            }
        }
        /// <summary>
        /// 验证三诺验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<SannuoSmsLoginReturnDto>> validSannuoSms(string phone,string code)
        {
            var res = await SannuoHelper.login(phone, code);
            if (!string.IsNullOrEmpty(res.access_token))
            {
                res.tokenExpire = DateTime.Now.AddSeconds(res.expires_in);
                return MessageModel<SannuoSmsLoginReturnDto>.Success("登录成功", res);
            }
            else
            {
                return MessageModel<SannuoSmsLoginReturnDto>.Fail("登录失败", res);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<GuardBloodInfo>>> getUserNowBloodList(long guardUserid)
        {
            var data = await _guardService.getUserNowBloodList(guardUserid);
            return MessageModel<List<GuardBloodInfo>>.Success("获取成功",data);
        }


        /// <summary>
        /// 获取监护账户列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<NightscoutGuardAccount>>> getGuardAccountList(int page, int size, string key)
        {
            var data = await _guardService.getGuardAccountList(page, size, key);

            return MessageModel<PageModel<NightscoutGuardAccount>>.Success("获取成功", data);
        }
        /// <summary>
        /// 添加监护监护账户
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<long>> addGuardAccount(NightscoutGuardAccount data)
        {
            var i = await _guardService.addGuardAccount(data);

            return MessageModel<long>.Success("添加成功", i);
        }

        /// <summary>
        /// 编辑监护监护账户
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<bool>> editGuardAccount(NightscoutGuardAccount data)
        {
            var i = await _guardService.editGuardAccount(data);

            return MessageModel<bool>.Success("编辑成功", i);
        }

        /// <summary>
        /// 删除监护监护账户
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<MessageModel<bool>> delGuardAccount(long id)
        {
            var i = await _guardService.delGuardAccount(id);

            return MessageModel<bool>.Success("删除成功", i);
        }


        /// <summary>
        /// 获取监护用户列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<NightscoutGuardUser>>> getGuardUserList(int page, int size, string key)
        {
            var data = await _guardService.getGuardUserList(page, size, key);

            return MessageModel<PageModel<NightscoutGuardUser>>.Success("获取成功", data);
        }
        /// <summary>
        /// 添加监护用户
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<long>> addGuardUser(NightscoutGuardUser data)
        {
            var i = await _guardService.addGuardUser(data);

            return MessageModel<long>.Success("添加成功", i);
        }

        /// <summary>
        /// 编辑监护用户
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<bool>> editGuardUser(NightscoutGuardUser data)
        {
            var i = await _guardService.editGuardUser(data);

            return MessageModel<bool>.Success("编辑成功", i);
        }

        /// <summary>
        /// 删除监护用户
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<MessageModel<bool>> delGuardUser(long id)
        {
            var i = await _guardService.delGuardUser(id);

            return MessageModel<bool>.Success("删除成功", i);
        }

        /// <summary>
        /// 获取所有nightscout用户
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<ShowKeyValueDto>>> getAllNsUser(int page=1,int size=10,string key="")
        {
            Expression<Func<Nightscout, bool>> whereExpression = a => true;
            if (!string.IsNullOrEmpty(key))
            {
                whereExpression = whereExpression.And(t => t.name.Contains(key) || t.url.Contains(key));
            }
            var ls = await _baseRepositoryNightscout.QueryPage(whereExpression, page, size);
            PageModel<ShowKeyValueDto> data = new PageModel<ShowKeyValueDto>();
            data.page = ls.page; 
            data.dataCount = ls.dataCount;
            data.size = ls.size;
            data.data = ls.data.Select(t => new ShowKeyValueDto { id = t.Id.ToString(), name = t.name ,url = t.url}).ToList();
            return MessageModel<PageModel<ShowKeyValueDto>>.Success("获取成功", data);
        }
        /// <summary>
        /// 获取所有监护账号
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<ShowKeyValueDto>>> getAllNsGuardAccount(int page=1, int size=10, string key="")
        {
            Expression<Func<NightscoutGuardAccount, bool>> whereExpression = a => true;
            if (!string.IsNullOrEmpty(key))
            {
                whereExpression = whereExpression.And(t => t.name.Contains(key));
            }
            var ls = await _baseRepositoryAccount.QueryPage(whereExpression, page, size);
            PageModel<ShowKeyValueDto> data = new PageModel<ShowKeyValueDto>();
            data.page = ls.page;
            data.dataCount = ls.dataCount;
            data.size = ls.size;
            data.data = ls.data.Select(t => new ShowKeyValueDto { id = t.Id.ToString(), name = t.name }).ToList();
            return MessageModel<PageModel<ShowKeyValueDto>>.Success("获取成功", data);
        }
        /// <summary>
        /// 获取所有监护用户
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<ShowKeyValueDto>>> getAllNsGuardUser(long gid,int page=1, int size=10, string key="")
        {
            PageModel<ShowKeyValueDto> data = await _guardService.getAllNsGuardUser(gid, page, size, key);

            return MessageModel<PageModel<ShowKeyValueDto>>.Success("获取成功", data);
        }
    }

}
