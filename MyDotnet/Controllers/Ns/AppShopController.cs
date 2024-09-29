using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.Apple;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.System;
using SixLabors.ImageSharp;
using System.Linq.Expressions;

namespace MyDotnet.Controllers.Ns
{

    /// <summary>
    /// app商店
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class AppShopController : BaseApiController
    {
        public DicService _dictService;
        private BaseServices<AppShopInfo> _appShopService;
        public AppShopController(DicService dictService
            , BaseServices<AppShopInfo> appShopService)
        {
            _dictService = dictService;
            _appShopService = appShopService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<MessageModel<PageModel<AppShopInfo>>> Get(string key,int page=1,int size=10,bool isOnlyEnable = false)
        {
            Expression<Func<AppShopInfo, bool>> whereExpression = a => true;
            if (!string.IsNullOrEmpty(key))
            {
                whereExpression = whereExpression.And(t => t.appName.Contains(key) || t.appDescription.Contains(key));
            }
            if (isOnlyEnable)
            {
                whereExpression = whereExpression.And(t => t.Enabled == isOnlyEnable);
            }
            var data =await _appShopService.Dal.QueryPage(whereExpression,page,size);
            return Success(data);
        } 
        [HttpPost]
        public async Task<MessageModel<long>> Post(AppShopInfo data)
        {
            var count = await _appShopService.Dal.Add(data);
            return Success(count);
        }
        [HttpPut]
        public async Task<MessageModel<bool>> Put(AppShopInfo data)
        {
            var count = await _appShopService.Dal.Update(data);
            return Success(count);
        }
        [HttpDelete]
        public async Task<MessageModel<bool>> Delete(long id)
        {
            var count = await _appShopService.Dal.DeleteById(id);
            return Success(count);
        }
    }

}
