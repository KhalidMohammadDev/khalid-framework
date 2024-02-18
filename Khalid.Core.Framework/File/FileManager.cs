using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Khalid.Core.Framework
{

    public static class FileManager
    {
        private static readonly string _baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");

        #region Private Methods

        private static string GetFileBase64String(string fileName, byte[] content)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType);
            return string.Format("data:{0};base64,{1}", contentType, Convert.ToBase64String(content));
        }


        public static async Task<string> GetFileBase64String(string fileSubPath)
        {
            string base64String = string.Empty;

            byte[] fileContent = await GetFile(fileSubPath);
            if (fileContent != null)
            {
                new FileExtensionContentTypeProvider().TryGetContentType(Path.GetFileName(fileSubPath), out string contentType);

                base64String = string.Format("data:{0};base64,{1}", contentType, Convert.ToBase64String(fileContent));
            }

            return base64String;
        }
        public static async Task<string> GetFileBase64StringApi(string fileSubPath)
        {
            string base64String = string.Empty;

            byte[] fileContent = await GetFile(fileSubPath);
            if (fileContent != null)
            {
                //new FileExtensionContentTypeProvider().TryGetContentType(Path.GetFileName(fileSubPath), out string contentType);

                base64String = string.Format("{0}", Convert.ToBase64String(fileContent));
            }

            return base64String;
        }


        public static byte[] GetFileBytes(string base64)
        {
            return Convert.FromBase64String(GetBase64String(base64));
        }

        public static string GetBase64String(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return base64;

            var v = base64.Split(",".ToCharArray());
            return v.Length == 1 ? v[0] : v[1];
        }

        #endregion Private Methods

        internal static async Task<string> SaveFile(Enum type, string fileName, byte[] fileByets, Stream fstream = null)
        {
            string currentDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileSubPath = Path.Combine(type.ToString(), String.Format("{0}-{1}", currentDate, fileName.Replace(" ", "_").Replace("+", "_").Replace("&", "_")));

            try
            {
                string fileFullPath = Path.Combine(_baseDirectory, fileSubPath);

                string directory = Path.GetDirectoryName(fileFullPath);

                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);

                using (FileStream stream = new FileStream(fileFullPath, FileMode.Create, FileAccess.Write))
                {
                    if (fileByets != null && fileByets.Length > 0)
                        await stream.WriteAsync(fileByets, 0, fileByets.Length);
                    else
                        await fstream.CopyToAsync(stream);
                }

            }
            catch
            {
                throw;
            }

            return fileSubPath;
        }

        public static async Task<List<string>> SaveMultiPartFiles(List<IFormFile> files, Enum type)
        {
            List<string> filePaths = new List<string>();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    filePaths.Add(await SaveFile(type, formFile.FileName, null, fstream: formFile.OpenReadStream()));
                }
            }

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return filePaths;
        }

        public static async Task<string> SaveFile(Enum type, string fileName, string base64)
        {
            return await SaveFile(type, fileName, GetFileBytes(base64));
        }
        public static async Task<string> SaveApiFile(Enum type, string fileName, string asdqqbase64, int index)
        {
            var fileSubPath = "";
            try
            {
                Random r = new Random();
                string currentDate = DateTime.Now.ToString("yyyyMMddHHmmss");
                fileSubPath = Path.Combine(type.ToString(), String.Format("{0}-{1}-{2}.{3}", fileName, currentDate, index, "jpeg"));

                string fileFullPath = Path.Combine(_baseDirectory, fileSubPath);

                string directory = Path.GetDirectoryName(fileFullPath);

                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);

                await File.WriteAllBytesAsync(fileFullPath, System.Convert.FromBase64String(asdqqbase64));
            }
            catch (Exception ex)
            {

            }

            return fileSubPath;
        }

        public static string GetFullPath(string fileSubPath)
        {
            return Path.Combine(_baseDirectory, fileSubPath);
        }
        public static async Task<byte[]> GetFile(string fileSubPath)
        {
            byte[] fileContent = null;

            string fileFullPath = GetFullPath(fileSubPath);
            if (System.IO.File.Exists(fileFullPath))
            {
                fileContent = await System.IO.File.ReadAllBytesAsync(fileFullPath);
            }

            return fileContent;
        }

        public static void DeleteFile(string fileSubPath)
        {
            string fileFullPath = Path.Combine(_baseDirectory, fileSubPath);
            if (System.IO.File.Exists(fileFullPath))
            {
                System.IO.File.Delete(fileFullPath);
            }
        }

        /// <summary>
        /// Get the name that is upload by user on ui
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileNameFromPath(string filePath)
        {
            string fileName = string.Empty;

            if (!string.IsNullOrEmpty(filePath))
            {
                int fileNameIndex = filePath.IndexOf('-');
                if (fileNameIndex > -1)
                {
                    fileName = filePath.Substring(fileNameIndex + 1);
                }
            }

            return fileName;
        }

        /// <summary>
        /// Get the generated file name by the system (that is stored on the disk and its part of the file path) 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetGeneratedFileNameFromPath(string filePath)
        {
            string realFileName = string.Empty;

            if (!string.IsNullOrEmpty(filePath))
            {
                int fileNameIndex = filePath.LastIndexOf('\\');
                if (fileNameIndex > -1)
                {
                    realFileName = filePath.Substring(fileNameIndex + 1);
                }
            }

            return realFileName;
        }

        /// <summary>
        /// Delete all files
        /// </summary>
        /// <param name="filesSubPaths">this variable contains multi files separated by |</param>
        public static void DeleteAllFiles(string filesSubPaths)
        {
            if (!String.IsNullOrWhiteSpace(filesSubPaths))
            {
                string[] filesPaths = filesSubPaths.Split('|');

                for (int i = 0; i < filesPaths.Count(); i++)
                {
                    FileManager.DeleteFile(filesPaths[i]);
                }
            }
        }

        /// <summary>
        /// Check if there are removed files from ui then delete those files from the disk and return the all un-removed files
        /// </summary>
        /// <param name="oldFilesSubPaths"></param>
        /// <param name="newFiles"></param>
        /// <returns></returns>
        public static List<string> DeleteRemovedFiles(string oldFilesSubPaths, List<FileModel> newFiles)
        {
            List<string> unRemovedFilesSubPaths = new List<string>();

            if (!String.IsNullOrWhiteSpace(oldFilesSubPaths))
            {
                List<string> oldFiles = oldFilesSubPaths.Split('|').ToList();

                if (newFiles != null && newFiles.Count > 0)
                {
                    for (int i = oldFiles.Count - 1; i >= 0; i--)
                    {
                        string oldFileSubPath = oldFiles[i];

                        // if old path is not exist on new files list, rhis means that this file is removed from ui
                        FileModel file = newFiles.FirstOrDefault(f => !String.IsNullOrEmpty(f.GeneratedName) && oldFileSubPath.EndsWith(f.GeneratedName));
                        if (file == null)
                        {
                            DeleteFile(oldFileSubPath);
                            oldFiles.Remove(oldFileSubPath);
                        }
                    }
                }

                unRemovedFilesSubPaths = oldFiles;
            }

            return unRemovedFilesSubPaths;
        }

        /// <summary>
        /// Save more than one file and return its paths joined by | separator
        /// </summary>
        /// <param name="fileType"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public static async Task<List<string>> SaveFiles(Enum fileType, List<FileModel> files)
        {
            List<string> filesSubPaths = new List<string>();

            if (files != null && files.Count > 0)
            {
                foreach (FileModel file in files)
                {
                    if (!String.IsNullOrWhiteSpace(file.Content) && IsBase64(file.Content))
                    {
                        filesSubPaths.Add(await FileManager.SaveFile(fileType, file.Name, file.Content));
                    }
                    else if (!String.IsNullOrWhiteSpace(file.Path))
                    {
                        filesSubPaths.Add(file.Path);
                    }
                    else
                    {
                        filesSubPaths.Add(Path.Combine(fileType.ToString(), file.GeneratedName));
                    }
                }
            }

            return filesSubPaths;
        }

        public static bool IsBase64(this string base64String)
        {
            // Credit: oybek https://stackoverflow.com/users/794764/oybek
            base64String = GetBase64String(base64String);
            if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0
               || base64String.Contains(" ") || base64String.Contains("\t") || base64String.Contains("\r") || base64String.Contains("\n"))
                return false;

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (Exception exception)
            {
                // Handle the exception
            }
            return false;
        }

        public static async Task<List<string>> SaveFiles(Enum fileType, string fileName, List<string> files)
        {
            List<string> filesSubPaths = new List<string>();
            foreach (var file in files)
            {
                filesSubPaths.Add(await FileManager.SaveFile(fileType, fileName, file));
            }
            return filesSubPaths;
        }
        public static async Task<List<string>> SaveApiFiles(Enum fileType, string fileName, List<string> files)
        {
            List<string> filesSubPaths = new List<string>();

            for (int i = 0; i < files.Count(); i++)
            {
                if (files[i] != "")
                {
                    filesSubPaths.Add(await FileManager.SaveApiFile(fileType, fileName, files[i], i + 1));

                }
            }
            return filesSubPaths;
        }
        public static async Task<string> SaveOrUpdateFile(string oldFilesPaths, List<FileModel> newFiles, Enum fileType)
        {
            List<string> newFilesPaths = new List<string>();

            // if there are old files stored on db
            //if (!String.IsNullOrWhiteSpace(oldFilesPaths))
            //{
            //    // Check if all files deleted from ui then delete all of them from disk
            //    if (newFiles == null || newFiles.Count == 0)
            //    {
            //        FileManager.DeleteAllFiles(oldFilesPaths);
            //    }
            //    else
            //    {
            //        newFilesPaths = FileManager.DeleteRemovedFiles(oldFilesPaths, newFiles);
            //    }
            //}

            // Save the new uploaded files
            if (newFiles != null && newFiles.Count > 0)
            {
                newFilesPaths = newFilesPaths.Concat(await FileManager.SaveFiles(fileType, newFiles)).ToList();
            }

            return newFilesPaths != null && newFilesPaths.Count > 0 ? string.Join("|", newFilesPaths) : null;
        }


    }
}

