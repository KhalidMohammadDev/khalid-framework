using Khalid.Core.Framework;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
namespace Khalid.Core.Framework
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private int[] Roles { get; }
        public AuthorizeAttribute(params int[] roles)
        {
            Roles = roles;
        }
        public AuthorizeAttribute()
        {
            Roles = new int[] { };
        }
        protected IUserEntity GetUserFromContext(AuthorizationFilterContext context)
        {
            return context.HttpContext.RequestServices.GetRequiredService<IAuthenticatedUserService>().GetAuthenticatedUser();
        }
        public virtual void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = GetUserFromContext(context);
            if (user == null || !context.HttpContext.RequestServices.GetRequiredService<IAuthenticatedUserService>().HasAnyPermission(Roles))
            {
                context.HttpContext.Response.StatusCode = 401;
                if (context.HttpContext.Request.Method.Equals("get", StringComparison.InvariantCultureIgnoreCase) &&
                    !(context.HttpContext.Request.Path.HasValue &&
                    context.HttpContext.Request.Path.Value.Contains("/api/", StringComparison.InvariantCultureIgnoreCase)))
                {
                    context.Result = new RedirectToActionResult("LogoutWeb", "Account", null);

                }
                else
                {
                    // not logged in
                    context.Result = new UnauthorizedResult();
                }
            }
        }
    }
}