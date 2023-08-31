using Microsoft.AspNetCore.Mvc;
using MyDotnet.Helper;

namespace MyDotnet.Controllers
{
    /// <summary>
    /// 测试控制器
    /// </summary>

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : Controller
    {
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
    }
}
