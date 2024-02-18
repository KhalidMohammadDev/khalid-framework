
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Khalid.Core.Framework
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;


        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            _ = await context.RequestServices?.GetRequiredService<IAuthenticatedUserService>().TryAttachUserToContext();

            await _next(context);

        }

    }
}
