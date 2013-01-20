using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using ermeX.Common;
using ermeX.NonMerged;

namespace ermeX.Tests.NonMerged
{
    [TestFixture]
    public class ResolveUnmergedTests
    {
        [TestFixtureSetUp]
        public void OnFixtureSetup()
        {
            ResolveUnmerged.Prepare();
        }

        [Test]
        public void CanResolveAssembly([Values( "Ninject", "System.Data.SQLite")] string assemblyName)
        {
            if (assemblyName == "System.Data.SQLite") //we do both tests as more than one test would have it for the next already loaded
            {
                string targetName = "SQLite.Interop.dll";
                string path = PathUtils.GetApplicationFolderPathFile(targetName);
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }catch(UnauthorizedAccessException ex)
                    {
                        Console.WriteLine(ex);
                        return;
                    }
                }
            }

            var assemblyRef = new AssemblyName(assemblyName);
            var actual = Assembly.Load(assemblyRef);
            Assert.IsNotNull(actual);
            if (assemblyName == "System.Data.SQLite") //we do both tests as more than one test would have it for the next already loaded
            {
                string targetName = "SQLite.Interop.dll";
                string path = PathUtils.GetApplicationFolderPathFile(targetName);
                bool exists = File.Exists(path);
                Assert.IsTrue(exists);
            }
        }
    }
    
}
