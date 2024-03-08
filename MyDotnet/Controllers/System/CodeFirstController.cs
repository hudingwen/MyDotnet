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
        [HttpPost]
        public async Task<MessageModel<string>> CreateTables([FromBody]DbFirstDTO dbFirstDto)
        {
            if (!_env.IsDevelopment())
                return Failed("当前不处于开发模式,请勿操作");
            await _codeFirstService.CreateTables(dbFirstDto);
            return Success("");
        }
    }
}
