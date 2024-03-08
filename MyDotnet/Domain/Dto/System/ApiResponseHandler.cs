using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using MyDotnet.Helper;
using System.Text.Encodings.Web;

namespace MyDotnet.Domain.Dto.System
{
    /// <summary>
    /// 自定义状态回写
    /// </summary>
    public class ApiResponseHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public ApiResponseHandler(IOptionsMonitor<AuthenticationSchemeOptions> options
            , ILoggerFactory logger
            , UrlEncoder encoder
            , ISystemClock clock)
            : base(options, logger, encoder, clock)
        {

        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.ContentType = "application/json";
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            await Response.WriteAsync(JsonHelper.ObjToJson(MessageModel.Fail("请登录")));
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.ContentType = "application/json";
            Response.StatusCode = StatusCodes.Status403Forbidden;
            await Response.WriteAsync(JsonHelper.ObjToJson(MessageModel.Fail("无权访问")));
        }
    }
}
