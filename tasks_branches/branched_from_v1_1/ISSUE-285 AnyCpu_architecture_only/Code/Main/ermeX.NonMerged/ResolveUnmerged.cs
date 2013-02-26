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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;

namespace ermeX.NonMerged
{
    internal static class ResolveUnmerged
    {


        private enum DataType
        {
            /// <summary>
            /// The assembly is not specialised depending on the build
            /// </summary>
            Any = 1,

            /// <summary>
            /// The assembly is different for each build x86,x64
            /// </summary>
            Specialized = 2,

        }
        private static readonly Dictionary<string,DataType> UnmergedAssemblies=new Dictionary<string,DataType>();
        private static readonly Dictionary<string, string> ToCopy = new Dictionary<string, string>(); 

        static ResolveUnmerged()
        {
            UnmergedAssemblies.Add("Common.Logging",DataType.Any);
            UnmergedAssemblies.Add("log4net", DataType.Any);

            UnmergedAssemblies.Add("System.Data.SQLite", DataType.Specialized);
            ToCopy.Add("System.Data.SQLite","SQLite.Interop");
        }

        public static void Init()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Dictionary<string, string>.ValueCollection valueCollection = ToCopy.Values;
            var applicationFolderPath = PathUtils.GetApplicationFolderPath();
            foreach (var value in valueCollection)
            {
                string filename = Path.Combine(applicationFolderPath, string.Format("{0}.dll", value));
                if(File.Exists(filename))
                {
                    try
                    {
                        File.Delete(filename);
                    }catch(Exception) 
                    {}
                }
            }
        }


        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(',')
                                 ? args.Name.Substring(0, args.Name.IndexOf(','))
                                 : args.Name.Replace(".dll", "");
            
                if (!UnmergedAssemblies.ContainsKey(dllName)) return null;

                string ns = "ermeX.NonMerged.Resources";
                if (UnmergedAssemblies[dllName] == DataType.Specialized)
                {
                    ns += Is64Bit ? ".x64" : ".x86";
                }
                //copy files if needed
                string applicationFolderPath;
                if (ToCopy.ContainsKey(dllName))
                {
                    applicationFolderPath = PathUtils.GetApplicationFolderPath();

                    string filename = Path.Combine(applicationFolderPath, ToCopy[dllName] + ".dll");
                    var o = ReadResourceBytes(string.Format("{0}.{1}.dll", ns, ToCopy[dllName]));
                    if (!File.Exists(filename))
                        using (var fs = new FileStream(filename, FileMode.Create))
                        {
                            fs.Write(o, 0, o.Length);
                        }
                }

                //load assembly

                var resName = string.Format("{0}.{1}.dll", ns, dllName);
                byte[] bytes = ReadResourceBytes(resName);
                return Assembly.Load(bytes);
           
        }

        private static byte[] ReadResourceBytes(string resName)
        {
            byte[] bytes;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName))
            {
                if (stream == null)
                    throw new ApplicationException(string.Format("The embedded resource {0} is missing.", resName));
                bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
            }
            return bytes;
        }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo);

        private static bool? _is64Bit=null ;

        private static bool Is64Bit
        {
            get
            {
                if (!_is64Bit.HasValue)
                    _is64Bit = IntPtr.Size == 8 || (IntPtr.Size == 4 && !Is32BitProcessOn64BitProcessor());

                return _is64Bit.Value;
            }
        }

        private static bool Is32BitProcessOn64BitProcessor()
        {
            bool retVal;

            IsWow64Process(Process.GetCurrentProcess().Handle, out retVal);

            return retVal;
        }
    }
}
