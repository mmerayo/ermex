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
	//[Explicit("TODO: invoke nunit alone for this one or change the process model")]
	[TestFixture]
	public sealed class ResolveUnmergedTests
	{
		[TestFixtureSetUp]
		public void OnFixtureSetUp()
		{
			PathUtils.CopyFolder(PathUtils.GetApplicationFolderPath(),UnmergedAssembliesTestContext.TestFilesFolder);
		}

		[TestFixtureTearDown]
		public void OnFixtureTearDown()
		{
			Directory.Delete(UnmergedAssembliesTestContext.TestFilesFolder,true);
		}

		[SetUp]
		public void OnSetup()
		{
			RemoveAssemblies();
		}
		
		[TearDown]
		public void OnTearDown()
		{
			UnmergedAssembliesTestContext.DisposeDomains();
		}

		private void RemoveAssemblies()
		{
			var lst = new List<String> {"Common.Logging.dll", "log4net.dll", "System.Data.SQLite.dll", "SQLite.Interop.dll"};
			foreach (var targetName in lst)
			{
				string path = Path.Combine(UnmergedAssembliesTestContext.TestFilesFolder, targetName);
				if (File.Exists(path))
					File.Delete(path);
			}
		}

		[Test]
		public void CanResolveAssembly([Values("Common.Logging", "log4net", "System.Data.SQLite")] string assemblyName,[Values (1,2,5)]int numberOfDomains)
		{
			var helpers = new UnmergedAssembliesTestContext[numberOfDomains];
			for (int i = 0; i < numberOfDomains; i++)
			{
				helpers[i] = UnmergedAssembliesTestContext.GetHelper();
				helpers[i].Init();
			}
			for (int i = 0; i < numberOfDomains; i++)
			{
				var canLoad = helpers[i].CanLoadAssembly(assemblyName);
				Assert.IsTrue(canLoad);

				if (assemblyName == "System.Data.SQLite")
				{
					const string targetName = "SQLite.Interop.dll";
					string path = Path.Combine(UnmergedAssembliesTestContext.TestFilesFolder, targetName);
					bool exists = File.Exists(path);
					Assert.IsTrue(exists);
				}
			}
		}

		private class UnmergedAssembliesTestContext : MarshalByRefObject
		{
			public static string TestFilesFolder
			{
				get
				{
					return PathUtils.GetApplicationFolderPath("ResolveUnmergedTest");
				}
			}

			private static readonly List<AppDomain> LoadedDomains = new List<AppDomain>();

			public static UnmergedAssembliesTestContext GetHelper()
			{
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				string pathToTheDll = Path.Combine(TestFilesFolder,executingAssembly.GetName().Name+".dll");
				AppDomain myDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString().Replace('-', '_'), null,
				                                            new AppDomainSetup()
					                                            {
						                                            ApplicationBase = Path.GetDirectoryName(pathToTheDll)
					                                            });
				LoadedDomains.Add(myDomain);

				var wrappedResult = myDomain.CreateInstanceFrom(pathToTheDll, typeof (UnmergedAssembliesTestContext).FullName);

				var result = (UnmergedAssembliesTestContext) wrappedResult.Unwrap();

				return result;
			}

			public static void DisposeDomains()
			{
				while (LoadedDomains.Count > 0)
				{
					try
					{
						AppDomain.Unload(LoadedDomains[0]);
					}
					catch (CannotUnloadAppDomainException)
					{
					}
					finally
					{
						LoadedDomains.RemoveAt(0);
					}
				}
			}

			public void Init()
			{
				ResolveUnmerged.Init();
			}

			public bool CanLoadAssembly(string assemblyName)
			{
				var assemblyRef = new AssemblyName(assemblyName);
				Assembly loadAssembly = Assembly.Load(assemblyRef);
				return loadAssembly!=null;
			}
		}
	}

}
