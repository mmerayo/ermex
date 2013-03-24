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
            ResolveUnmerged.Init();
        }

        [Test]
        public void CanResolveAssembly([Values("Common.Logging", "log4net", "System.Data.SQLite")] string assemblyName)
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
            //TODO: INVOKE TYPE FROM aSSEMBLY TO FORCE RESOLUTION AND UNCOMMENT THE FOLLOWING LINES
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
