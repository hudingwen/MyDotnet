using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Repository;
using MyDotnet.Services.System;
using SqlSugar;

namespace MyDotnet.Controllers.System
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CodeFirstController : BaseApiController
    {

        private readonly CodeFirstService _codeFirstService;
        private readonly IWebHostEnvironment _env;
        public CodeFirstController(CodeFirstService codeFirstService, IWebHostEnvironment env)
        {
            _codeFirstService = codeFirstService;
            _env = env;
        }
        /// <summary>
        /// 创建一个数据库表
        /// </summary>
        /// <param name="codeFirstDTO"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> CreateTables([FromBody]CodeFirstDTO codeFirstDTO)
        {
            if (!_env.IsDevelopment())
                return Failed("当前不处于开发模式,请勿操作");
            await _codeFirstService.CreateTables(codeFirstDTO);
            return Success("创建成功");
        }
        /// <summary>
        /// 保存数据库种子数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> SaveSeedData()
        {
            if (!_env.IsDevelopment())
                return Failed("当前不处于开发模式,请勿操作");
            await _codeFirstService.SaveSeedData();
            return Success("保存成功");
        }
    }
}
