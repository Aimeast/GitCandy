using GitCandy.Configuration;
using GitCandy.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace GitCandy.Filters
{
    public class AllowRegisterUserFilter : SmartAuthorizeFilter
    {
        private IServiceProvider _serviceProvider;

        public AllowRegisterUserFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void OnResourceExecuting(ResourceExecutingContext context)
        {
            base.OnResourceExecuting(context);

            var token = context.HttpContext.Features.Get<Token>();
            var settings = _serviceProvider.GetService<IOptions<UserSettings>>();
            if (token != null || !settings.Value.AllowRegisterUser)
            {
                HandleUnauthorizedRequest(context);
            }
        }
    }
}
