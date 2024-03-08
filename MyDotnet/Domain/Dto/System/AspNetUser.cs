using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyDotnet.Helper;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace MyDotnet.Domain.Dto.System
{
    public class AspNetUser
    {
        private readonly IHttpContextAccessor _accessor;

        public AspNetUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string Name => GetName();

        private string GetName()
        {
            if (IsAuthenticated() && _accessor.HttpContext.User.Identity.Name.IsNotEmptyOrNull())
            {
                return _accessor.HttpContext.User.Identity.Name;
            }
            else
            {
                if (!string.IsNullOrEmpty(GetToken()))
                {
                    return GetUserInfoFromToken(ClaimTypes.Name).FirstOrDefault().ObjToString();
                }
            }

            return "";
        }

        public long ID => GetClaimValueByType(JwtRegisteredClaimNames.Jti).FirstOrDefault().ObjToLong();
        public long TenantId => GetClaimValueByType("TenantId").FirstOrDefault().ObjToLong();

        public bool IsAuthenticated()
        {
            return _accessor.HttpContext != null && _accessor.HttpContext.User.Identity.IsAuthenticated;
        }


        public string GetToken()
        {
            var token = _accessor.HttpContext?.Request?.Headers["Authorization"].ObjToString().Replace("Bearer ", "");
            return token;
        }

        public List<string> GetUserInfoFromToken(string ClaimType)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = "";

            token = GetToken();
            // token校验
            if (token.IsNotEmptyOrNull() && jwtHandler.CanReadToken(token))
            {
                JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(token);

                return (from item in jwtToken.Claims
                        where item.Type == ClaimType
                        select item.Value).ToList();
            }

            return new List<string>() { };
        }

        public MessageModel<string> MessageModel { get; set; }

        public IEnumerable<Claim> GetClaimsIdentity()
        {
            if (_accessor.HttpContext == null) return ArraySegment<Claim>.Empty;

            if (!IsAuthenticated()) return GetClaimsIdentity(GetToken());

            var claims = _accessor.HttpContext.User.Claims.ToList();
            var headers = _accessor.HttpContext.Request.Headers;
            foreach (var header in headers)
            {
                claims.Add(new Claim(header.Key, header.Value));
            }

            return claims;
        }

        public IEnumerable<Claim> GetClaimsIdentity(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            // token校验
            if (token.IsNotEmptyOrNull() && jwtHandler.CanReadToken(token))
            {
                var jwtToken = jwtHandler.ReadJwtToken(token);

                return jwtToken.Claims;
            }

            return new List<Claim>();
        }

        public List<string> GetClaimValueByType(string ClaimType)
        {
            return (from item in GetClaimsIdentity()
                    where item.Type == ClaimType
                    select item.Value).ToList();
        }
    }
}