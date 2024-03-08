using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Common.Cache;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Services.System;

namespace MyDotnet.Controllers.System
{
    /// <summary>
    /// 测试控制器
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController] 
    public class TestController : BaseApiController
    {
        ICaching caching;
        public TestController(ICaching _caching) {
            caching = _caching;
        }
        /// <summary>
        /// 获取一个缓存
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> test(string code)
        {
            var data = await caching.GetAsync<string>(code);
            return Success(data);
        }
        /// <summary>
        /// 设置一个缓存
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> test2(string code,string data)
        {
            await caching.SetAsync<string>(code, data);
            return Success("","成功");
        }
    }
}
