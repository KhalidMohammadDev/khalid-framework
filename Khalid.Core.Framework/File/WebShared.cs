using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Khalid.Core.Framework
{
    public class WebShared
    {
        public WebShared(IHttpContextAccessor context)
        {
            CurrentContext = context.HttpContext;
        }

        public HttpContext CurrentContext;


        public string AppBaseUrl(HttpContext context)
        {
            return $"{context.Request.Scheme}://{context.Request.Host}";
        }
        public IEnumerable<string> GetFileUrls(string paths)
        {
            if (!string.IsNullOrWhiteSpace(paths) && CurrentContext != null)
            {
                var context = CurrentContext;
                var urlHelperFactory = context.RequestServices.GetRequiredService<IUrlHelperFactory>();
                var actionContext = context.RequestServices.GetRequiredService<IActionContextAccessor>().ActionContext;
                var urlHelper = urlHelperFactory.GetUrlHelper(actionContext);

                return paths.Split("|").Select(s => AppBaseUrl(context) + urlHelper.Action("DownloadFile", "File", new { filePath = s }));
            }

            return null;
        }
    }
}
