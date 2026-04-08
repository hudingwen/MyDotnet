
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Helper;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MyDotnet.Controllers.System
{
    /// <summary>
    /// 服务器信息管理
    /// </summary>
    [Route("api/[Controller]/[action]")]
    [ApiController]
    [Authorize]
    public class MonitorController : BaseApiController
    {
        IHostEnvironment _hostEnvironment;
        IWebHostEnvironment _webHostEnvironment;
        public MonitorController(IHostEnvironment hostEnvironment, IWebHostEnvironment webHostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _webHostEnvironment = webHostEnvironment;
        }
        /// <summary>
        /// 服务器配置信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public MessageModel<ServerViewModel> Server()
        {
            return Success(new ServerViewModel()
            {
                EnvironmentName = _hostEnvironment.EnvironmentName,
                OSArchitecture = RuntimeInformation.OSArchitecture.ObjToString(),
                ContentRootPath = _hostEnvironment.ContentRootPath,
                WebRootPath = _webHostEnvironment.WebRootPath,
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                MemoryFootprint = (Process.GetCurrentProcess().WorkingSet64 / 1048576).ToString("N2") + " MB",
                WorkingTime = DateHelper.TimeSubTract(DateTime.Now, Process.GetCurrentProcess().StartTime)
            }, "");
        }
    }
}
