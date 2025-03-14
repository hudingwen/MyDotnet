using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using MyDotnet.Controllers.Ns;
using MyDotnet.Domain.Dto.Hospital;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Hospital;
using MyDotnet.Domain.Entity.Nginx;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Repository.System;
using MyDotnet.Services.Analyze;
using MyDotnet.Services.System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace MyDotnet.Controllers.Base
{
    /// <summary>
    /// Nginx日志分析
    /// </summary>

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StudyController : BaseApiController
    {
        public UnitOfWorkManage _unitOfWorkManage;
        public BaseRepository<StudyCategory> _category;
        public BaseRepository<StudyParameter> _parameter;
        /// <summary>
        /// 构造函数
        /// </summary>
        public StudyController(UnitOfWorkManage unitOfWorkManage
            , BaseRepository<StudyCategory> category
            , BaseRepository<StudyParameter> parameter
            )
        {
            _unitOfWorkManage = unitOfWorkManage;
            _category = category;
            _parameter = parameter;
        } 

        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<StudyDto>> Get(long pid = 0)
        {
            var categoryInfo =  await _category.QueryById(pid);
            var categoryChildList = await _category.Query(t => t.categoryParentId == pid);
            var parameterList = await _parameter.Query(t => t.categoryId == pid);
            StudyDto studyDto = new StudyDto();
            studyDto.categoryInfo = categoryInfo;
            studyDto.categoryChildList = categoryChildList;
            studyDto.parameterList = parameterList;
            return Success(studyDto);
        }

    }
}
