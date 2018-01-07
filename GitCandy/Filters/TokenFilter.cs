using GitCandy.Data;
using GitCandy.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace GitCandy.Filters
{
    public class TokenFilter : IResourceFilter, IResultFilter
    {
        private const string AuthCookieKey = "_gc_auth";

        private readonly DataService _dataService;

        public TokenFilter(DataService dataService)
        {
            _dataService = dataService;
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var cookie = context.HttpContext.Request.Cookies[AuthCookieKey];
            if (cookie != null)
            {
                var bytes = Convert.FromBase64String(cookie);
                var guid = new Guid(bytes);
                var token = _dataService.UserManager.GetAuthorizationToken(guid);

                if (token != null && !token.Expired)
                {
                    context.HttpContext.Features.Set(token);
                }
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var token = context.HttpContext.Features.Get<Token>();
            if (token != null && !token.Expired)
            {
                var ip = context.HttpContext
                    .Features
                    .Get<IHttpConnectionFeature>()
                    ?.RemoteIpAddress
                    ?.ToString();

                if (token.RenewIfNeed() || token.LastIp != ip)
                {
                    token.LastIp = ip;
                    _dataService.UserManager.UpdateAuthorization(token.AuthCode, token.Expires, token.LastIp);
                }

                context.HttpContext.Response.Cookies.Append(AuthCookieKey,
                    Convert.ToBase64String(token.AuthCode.ToByteArray()),
                    new CookieOptions { Expires = token.Expires });
            }
            else
            {
                context.HttpContext.Response.Cookies.Delete(AuthCookieKey);
            }
        }
    }
}
