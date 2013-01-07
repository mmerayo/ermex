// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.IO;
using System.Reflection;

namespace ermeX.Common
{
    public static class PathUtils
    {
        private static string _applicationFolderPath;

        public static string GetApplicationFolderPath(string folderName)
        {
            return string.Format("{0}\\{1}", GetApplicationFolderPath(), folderName);
        }

        public static string GetApplicationFolderPath()
        {
            if (_applicationFolderPath == null)
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                _applicationFolderPath = Path.GetDirectoryName(path);
            }
            return _applicationFolderPath;
        }

        public static string GetPath(string uri)
        {
            var uriBuilder = new UriBuilder(uri);
            string path = Uri.UnescapeDataString(uriBuilder.Path);
            _applicationFolderPath = Path.GetDirectoryName(path);
            return _applicationFolderPath;
        }

        public static string GetApplicationFolderPathFile(string fileName)
        {
            return Path.Combine(GetApplicationFolderPath(), fileName);
        }
    }
}