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
}
