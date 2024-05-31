using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using MyDotnet.Controllers.Ns;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.System;
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
    public class NginxController : BaseApiController
    {
        public UnitOfWorkManage _unitOfWorkManage;
        public BaseRepository<NginxHostRequest> _baseRepositoryHost;
        public BaseRepository<NginxHostUrlRequest> _baseRepositoryUrl;
        /// <summary>
        /// 构造函数
        /// </summary>
        public NginxController(UnitOfWorkManage unitOfWorkManage
            , BaseRepository<NginxHostRequest> baseRepositoryHost
            , BaseRepository<NginxHostUrlRequest> baseRepositoryUrl
            )
        {
            _unitOfWorkManage = unitOfWorkManage;
            _baseRepositoryHost = baseRepositoryHost;
            _baseRepositoryUrl = baseRepositoryUrl;
        } 
        /// <summary>
        /// 获取访问日志
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<NginxHostRequest>>> Get(int page = 1, int size = 10, string key = "")
        {
            Expression<Func<NginxHostRequest, bool>> whereExpression = a => true;
            if (!string.IsNullOrEmpty(key))
            {
                whereExpression = whereExpression.And(t => t.host.Contains(key));
            }
            var data = await _baseRepositoryHost.QueryPage(whereExpression, page, size);
            return Success(data);
        }

    }
}
