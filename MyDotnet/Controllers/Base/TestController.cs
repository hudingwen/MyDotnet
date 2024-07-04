using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using MongoDB.Bson;
using MongoDB.Driver;
using MyDotnet.Controllers.Ns;
using MyDotnet.Domain.Dto.Guiji;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Repository.System;
using MyDotnet.Services.System;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MyDotnet.Controllers.Base
{
    /// <summary>
    /// 测试控制器
    /// </summary>

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : Controller
    {
        public UnitOfWorkManage _unitOfWorkManage;
        public BaseRepository<SysUserInfo> _baseRepositoryUser;
        public IWebHostEnvironment _env;
        public RoleModulePermissionRepository _roleModulePermissionRepository;
        public RoleModulePermissionServices _roleModulePermissionServices;
        /// <summary>
        /// 构造函数
        /// </summary>
        public TestController(UnitOfWorkManage unitOfWorkManage
            , BaseRepository<SysUserInfo> baseRepositoryUser
            , IWebHostEnvironment env
            , RoleModulePermissionRepository roleModulePermissionRepository
            , RoleModulePermissionServices roleModulePermissionServices
            )
        {
            _unitOfWorkManage = unitOfWorkManage;
            _baseRepositoryUser = baseRepositoryUser;
            _env = env;
            _roleModulePermissionRepository = roleModulePermissionRepository;
            _roleModulePermissionServices = roleModulePermissionServices;
        }
        /// <summary>
        /// 服务器配置信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ServerViewModel Server()
        {
            var data = new ServerViewModel()
            {
                EnvironmentName = _env.EnvironmentName,
                OSArchitecture = RuntimeInformation.OSArchitecture.ObjToString(),
                ContentRootPath = _env.ContentRootPath,
                WebRootPath = _env.WebRootPath,
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                MemoryFootprint = (Process.GetCurrentProcess().WorkingSet64 / 1048576).ToString("N2") + " MB",
                WorkingTime = DateHelper.TimeSubTract(DateTime.Now, Process.GetCurrentProcess().StartTime)
            };

            return data;
        }
        /// <summary>
        /// 日志测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string Get()
        {
            LogHelper.logApp.Debug("00000");
            LogHelper.logApp.Info("11111");
            LogHelper.logApp.Warn("22222");
            LogHelper.logApp.Error("33333");
            LogHelper.logApp.Fatal("44444");
            return "OK";
        }
        /// <summary>
        /// long转string测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public long Get2()
        {
            return 123456;
        }

        /// <summary>
        /// 事务测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public void Get3()
        {
            _unitOfWorkManage.BeginTran();
            _unitOfWorkManage.CommitTran();
        }
        /// <summary>
        /// 泛型仓储测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<SysUserInfo>> Get4()
        {
            return await _baseRepositoryUser.Query();
        }
        /// <summary>
        /// 全局异常测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<SysUserInfo>> Get5()
        {
            await Task.CompletedTask;
            throw new Exception("测试异常");
        }
        /// <summary>
        /// 自定仓层储测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<RoleModulePermission>> Get6()
        {
            var ls = await _roleModulePermissionRepository.Query();
            return ls;
        }
        /// <summary>
        /// 自服务层储测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<RoleModulePermission>> Get7()
        {
            var ls = await _roleModulePermissionServices.Dal.Query();
            return ls;
        } 

        [HttpGet]
        [AllowAnonymous]
        public async Task<object> MyTest()
        {
            return null;
        }

    } 
}
