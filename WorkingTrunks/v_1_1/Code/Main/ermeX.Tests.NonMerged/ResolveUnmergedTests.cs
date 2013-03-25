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
	public sealed class ResolveUnmergedTests
	{
		[SetUp]
		public void OnSetup()
		{
			RemoveAssemblies();
		}

		private void RemoveAssemblies()
		{
			var lst = new List<String> {"Common.Logging.dll", "log4net.dll", "System.Data.SQLite.dll", "SQLite.Interop.dll"};
			foreach (var targetName in lst)
			{
				string path = PathUtils.GetApplicationFolderPathFile(targetName);
				if (File.Exists(path))
					File.Delete(path);
			}
		}

		[TearDown]
		public void OnTearDown()
		{
			UnmergedAssembliesTestHelper.DisposeDomains();
		}

		[Test]
		public void CanResolveAssembly([Values("Common.Logging", "log4net", "System.Data.SQLite")] string assemblyName,[Values (1,2,5)]int numberOfDomains)
		{
			var helpers = new UnmergedAssembliesTestHelper[numberOfDomains];
			for (int i = 0; i < numberOfDomains; i++)
			{
				helpers[i] = UnmergedAssembliesTestHelper.GetHelper();
				helpers[i].Init();
			}
			for (int i = 0; i < numberOfDomains; i++)
			{
				var canLoad = helpers[i].CanLoadAssembly(assemblyName);
				Assert.IsTrue(canLoad);

				if (assemblyName == "System.Data.SQLite")
				{
					string targetName = "SQLite.Interop.dll";
					string path = PathUtils.GetApplicationFolderPathFile(targetName);
					bool exists = File.Exists(path);
					Assert.IsTrue(exists);
				}
			}
		}

		private class UnmergedAssembliesTestHelper : MarshalByRefObject
		{
			private static readonly List<AppDomain> LoadedDomains = new List<AppDomain>();

			public static UnmergedAssembliesTestHelper GetHelper()
			{
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				string pathToTheDll = Path.Combine(PathUtils.GetPath(executingAssembly.CodeBase),executingAssembly.GetName().Name+".dll");
				AppDomain myDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString().Replace('-', '_'), null,
				                                            new AppDomainSetup()
					                                            {
						                                            ApplicationBase = Path.GetDirectoryName(pathToTheDll)
					                                            });
				LoadedDomains.Add(myDomain);

				var wrappedResult = myDomain.CreateInstanceFrom(pathToTheDll, typeof (UnmergedAssembliesTestHelper).FullName);

				var result = (UnmergedAssembliesTestHelper) wrappedResult.Unwrap();

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
