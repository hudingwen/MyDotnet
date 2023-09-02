using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using MyDotnet.Helper;
using MyDotnet.Repository.System;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace MyDotnet.Domain.Dto.System
{
    /// <summary>
    /// 自定义验证权限
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        /// <summary>
        /// 验证方案提供对象
        /// </summary>
        public IAuthenticationSchemeProvider _schemes;
        /// <summary>
        /// 上下文
        /// </summary>
        public IHttpContextAccessor _accessor;
        /// <summary>
        /// 权限
        /// </summary>
        public RoleModulePermissionRepository _roleModulePermissionRepository;

        public PermissionHandler(IHttpContextAccessor accessor
            , IAuthenticationSchemeProvider schemes
            , RoleModulePermissionRepository roleModulePermissionRepository)
        {
            _accessor = accessor;
            _schemes = schemes;
            _roleModulePermissionRepository = roleModulePermissionRepository;
        }

        // 重写异步处理程序
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var httpContext = _accessor.HttpContext;

            // 获取系统中所有的角色和菜单的关系集合
            if (!requirement.Permissions.Any())
            {
                var data = await _roleModulePermissionRepository.RoleModuleMaps();
                var list = new List<PermissionItem>();
                list = (from item in data
                        where item.IsDeleted == false
                        orderby item.Id
                        select new PermissionItem
                        {
                            Url = item.Module?.LinkUrl,
                            Role = item.Role?.Name
                            //Role = item.Role?.Id.ObjToString(),
                        }).ToList();
                requirement.Permissions = list;
            }

            //判断用户是否具有权限
            if (httpContext != null)
            {
                var questUrl = httpContext.Request.Path.Value.ToLower();

                // 整体结构类似认证中间件UseAuthentication的逻辑，具体查看开源地址
                // https://github.com/dotnet/aspnetcore/blob/master/src/Security/Authentication/Core/src/AuthenticationMiddleware.cs
                httpContext.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
                {
                    OriginalPath = httpContext.Request.Path,
                    OriginalPathBase = httpContext.Request.PathBase
                });

                // Give any IAuthenticationRequestHandler schemes a chance to handle the request
                // 主要作用是: 判断当前是否需要进行远程验证，如果是就进行远程验证
                var handlers = httpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
                foreach (var scheme in await _schemes.GetRequestHandlerSchemesAsync())
                {
                    if (await handlers.GetHandlerAsync(httpContext, scheme.Name) is IAuthenticationRequestHandler
                            handler && await handler.HandleRequestAsync())
                    {
                        context.Fail();
                        return;
                    }
                }

                //判断请求是否拥有凭据，即有没有登录
                var defaultAuthenticate = await _schemes.GetDefaultAuthenticateSchemeAsync();
                if (defaultAuthenticate != null)
                {
                    var result = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);

                    //result?.Principal不为空即登录成功
                    if (result?.Principal != null)
                    {
                        var isExp = true;
                        var exp = httpContext.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.Expiration);

                        if (exp != null)
                        {
                            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(exp.Value.ObjToLong());
                            DateTime dateTime = dateTimeOffset.LocalDateTime;

                            if (DateTime.Now > dateTime)
                                isExp = true;
                            else
                                isExp = false;
                        }


                        if (isExp)
                        {
                            context.Fail(new AuthorizationFailureReason(this, "授权已过期,请重新授权"));
                            return;
                        }


                        var currentUserRoles = (from item in httpContext.User.Claims
                                                where item.Type == requirement.ClaimType
                                                select item.Value).ToList();

                        //超级管理员 默认拥有所有权限
                        if (currentUserRoles.All(s => s != "SuperAdmin"))
                        {
                            var isMatchRole = false;
                            var permisssionRoles =
                                requirement.Permissions.Where(w => currentUserRoles.Contains(w.Role));
                            foreach (var item in permisssionRoles)
                            {
                                try
                                {
                                    if (Regex.Match(questUrl, item.Url?.ObjToString().ToLower())?.Value == questUrl)
                                    {
                                        isMatchRole = true;
                                        break;
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }

                            //验证权限
                            if (currentUserRoles.Count <= 0 || !isMatchRole)
                            {
                                context.Fail();
                                return;
                            }
                        }

                        context.Succeed(requirement);
                        return;
                    }
                }

                //判断没有登录时，是否访问登录的url,并且是Post请求，并且是form表单提交类型，否则为失败
                if (!(questUrl.Equals(requirement.LoginPath.ToLower(), StringComparison.Ordinal) &&
                      (!httpContext.Request.Method.Equals("POST") || !httpContext.Request.HasFormContentType)))
                {
                    context.Fail();
                    return;
                }
            }
        }
    }
}
