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
            Any = 1,

            /// <summary>
            /// The assembly is different for each build
            /// </summary>
            Specialized = 2,

        }
        private static readonly Dictionary<string,DataType> UnmergedAssemblies=new Dictionary<string,DataType>();
        private static readonly Dictionary<string, string> ToCopy = new Dictionary<string, string>(); 

        static ResolveUnmerged()
        {
            UnmergedAssemblies.Add("Common.Logging",DataType.Any);
            UnmergedAssemblies.Add("Ninject", DataType.Any);

            UnmergedAssemblies.Add("System.Data.SQLite", DataType.Specialized);
            ToCopy.Add("System.Data.SQLite","SQLite.Interop");
        }

        public static void Prepare()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
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
