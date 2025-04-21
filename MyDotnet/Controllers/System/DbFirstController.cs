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
    public class DbFirstController : BaseApiController
    {

        private readonly DbFirstService _dbFirstService;
        private readonly IWebHostEnvironment _env;
        public DbFirstController(DbFirstService dbFirstService, IWebHostEnvironment env)
        {
            _dbFirstService = dbFirstService;
            _env = env;
        }
        /// <summary>
        /// 通过数据库表创建实体
        /// </summary>
        /// <param name="dbFirstDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> CreateModels([FromBody]DbFirstDTO dbFirstDto)
        {
            if (!_env.IsDevelopment())
                return Failed("当前不处于开发模式,请勿操作"); 
            await _dbFirstService.CreateModels(dbFirstDto);
            return Success("创建成功");
        }
    }
}
