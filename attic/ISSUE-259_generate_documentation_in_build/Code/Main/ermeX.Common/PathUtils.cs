// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
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