using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Khalid.Core.Framework
{
    public static class FileConverter
    {


        public static List<FileModel> GetFileModelFromJson(string fileJson)
        {

            return !String.IsNullOrEmpty(fileJson) ?
                         JsonConvert.DeserializeObject<List<FileModel>>(fileJson) : null;
        }

        public static async Task<string> SaveAndGetFilePathesFromModel(string fileJson, Enum fileType)
        {
            var files = GetFileModelFromJson(fileJson);

            return await FileManager.SaveOrUpdateFile(null, files, fileType);
        }

        public static string GetFileJson(string filePathes, WebShared webShared = null)
        {
            string fileJSon = null;
            if (!string.IsNullOrWhiteSpace(filePathes))
            {
                fileJSon = JsonConvert.SerializeObject(filePathes.Split("|").Select(f =>

                    new FileModel
                    {
                        Name = Path.GetFileName(f),
                        GeneratedName = Path.GetFileName(f),
                        Content = webShared?.GetFileUrls(f)?.FirstOrDefault(),
                        Path = f
                    }));

            }

            return fileJSon;
        }
    }

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
