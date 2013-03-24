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
        //TODO: rEWRITE OR REFACTOR THIS WITH PROPER UNIT TESTS

        private enum DataType
        {
            /// <summary>
            /// The assembly is not specialised depending on the build
            /// </summary>
            Any = 1,

            /// <summary>
            /// The assembly is different for each build x86,x64 TODO: 3.5,4.0..
            /// </summary>
            Specialized = 2,

            Unmanaged = 3,

        }

        private class UnmergedAssemblyInfo
        {
            public string Name { get; set; }
            public DataType Type { get; set; }
            public Version Version { get; set; }
            public readonly List<UnmergedAssemblyInfo> NonManagedToCopy=new List<UnmergedAssemblyInfo>(); 
        }
        private static readonly List<UnmergedAssemblyInfo> UnmergedAssemblies=new List<UnmergedAssemblyInfo>();
        
        static ResolveUnmerged()
        {
            UnmergedAssemblies.Add(new UnmergedAssemblyInfo{Name="Common.Logging",Type=DataType.Any,Version=new Version(2,1,2,0)});
            UnmergedAssemblies.Add(new UnmergedAssemblyInfo { Name = "log4net", Type = DataType.Any, Version = new Version(1, 2, 11, 0) });
            var sqlIte = new UnmergedAssemblyInfo { Name = "System.Data.SQLite", Type = DataType.Specialized, Version = new Version(1, 0,83,0) };
            UnmergedAssemblies.Add(sqlIte);
            sqlIte.NonManagedToCopy.Add(new UnmergedAssemblyInfo { Name = "SQLite.Interop", Type = DataType.Unmanaged, Version = new Version(1, 0, 83,0) });
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        public static void Init()
        {
            //forces the type cctor and the following
            RemoveResolvableAssemblies(); //this forces to update exiting to use the embedded version
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            //RemoveResolvableAssemblies();
        }

        private static void RemoveResolvableAssemblies()
        {
            var applicationFolderPath = PathUtils.GetApplicationFolderPath();

            foreach (var assembly in UnmergedAssemblies)
            {
                foreach(var cpy in assembly.NonManagedToCopy)
                    RemoveAssembly(cpy,applicationFolderPath);
                
                RemoveAssembly(assembly, applicationFolderPath);
                
            }
        }

        private static void RemoveAssembly(UnmergedAssemblyInfo value, string applicationFolderPath)
        {
            var assembly = TypesHelper.GetAssemblyFromDomain(value.Name,false);
            if (assembly == null || value.Type==DataType.Unmanaged || value.Type==DataType.Specialized)
            {
                string filename = Path.Combine(applicationFolderPath, string.Format("{0}.dll", value.Name));
                if (File.Exists(filename)) 
                {
                    try
                    {
                        File.Delete(filename);
                        Console.WriteLine("Deleted " + filename); //TODO: REMOVE THIS
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception deleting {0} {1}", filename, ex); //TODO: REMOVE THIS
                    }
                }
            }
            else
            {
                string s = assembly.FullName.Split(',')[1].Split('=')[1];
                var version = Version.Parse(s);
                if(version.CompareTo(value.Version)!=0)
                    throw new BadImageFormatException(string.Format("Version supported for assembly {0} is {1}. Current is {2}",value,value.Version,version));
            }
        }


        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(',')
                                 ? args.Name.Substring(0, args.Name.IndexOf(','))
                                 : args.Name.Replace(".dll", "");

            UnmergedAssemblyInfo info = UnmergedAssemblies.SingleOrDefault(x=>x.Name==dllName);
            if (info==null) return null;

            string ns = "ermeX.NonMerged.Resources";
            if (info.Type == DataType.Specialized)
            {
                ns += Is64Bit ? ".x64" : ".x86";
            }
            //copy files if needed
            string applicationFolderPath;
            foreach (var curr in info.NonManagedToCopy)
            {

                applicationFolderPath = PathUtils.GetApplicationFolderPath();

                string filename = Path.Combine(applicationFolderPath, curr.Name + ".dll");
                var o = ReadResourceBytes(string.Format("{0}.{1}.dll", ns, curr.Name));
                if (!File.Exists(filename))
                {
                    using (var fs = new FileStream(filename, FileMode.Create))
                        fs.Write(o, 0, o.Length);

                    Console.WriteLine("Copied " +filename); //TODO: REMOVE THIS
                }
                else
                {
                    Console.WriteLine("Not copied " + filename); //TODO: REMOVE THIS
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
