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
        [HttpPost]
        public async Task<MessageModel<string>> CreateModels([FromBody]DbFirstDTO dbFirstDto)
        {
            if (!_env.IsDevelopment())
                return Failed("当前不处于开发模式,请勿操作");
            //"kmkm", $@"C:/my-file/Blog.Core.Model", "Blog.Core.Model.Models",new string[] { "t_code" }
            await _dbFirstService.CreateModels(dbFirstDto);
            return Success("");
        }
    }
}
