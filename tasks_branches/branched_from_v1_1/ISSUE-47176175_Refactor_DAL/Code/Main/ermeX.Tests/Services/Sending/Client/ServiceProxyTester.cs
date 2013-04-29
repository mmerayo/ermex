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
using NUnit.Framework;
using ermeX.Common.Caching;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.DAL.DataAccess.Helpers;

using ermeX.Models.Entities;
using ermeX.LayerMessages;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.Services.Mock;

using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Publish;

namespace ermeX.Tests.Services.Sending.Client
{
	internal sealed class ServiceProxyTester : DataAccessTestBase
	{
		private ICacheProvider GetCacheProvider()
		{
			return new MemoryCacheStore(300); //TODO: REMOVE AS THE CACHING WILL BE RE-ADDED IN THE FUTURE
		}

		private ServiceProxy GetTarget(out MockConnectivityProvider provider,
		                               bool withEndPoints = true)
		{
			provider = GetDummyConnectivityProvider(withEndPoints);
			return
				new ServiceProxy(GetCacheProvider(), provider, TestSettingsProvider.GetClientConfigurationSettingsSource());
		}

		private ServiceProxy GetTarget(bool withEndPoints = true)
		{
			MockConnectivityProvider none;
			return GetTarget(out none, withEndPoints);
		}

		private MockConnectivityProvider GetDummyConnectivityProvider(bool withEndPoints)
		{
			return new MockConnectivityProvider(withEndPoints);
		}

		private TransportMessage GetTransportMessage<TData>(TData data)
		{
			var bizMessage = new BizMessage(data);
			var busMessage = new BusMessage(LocalComponentId, bizMessage);
			var transportMessage = new TransportMessage(RemoteComponentId, busMessage);
			return transportMessage;
		}


		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.OptionDbInMemory)]
		public void CanInvokeSend(DbEngineType engineType)
		{
			using (ServiceProxy target = GetTarget())
			{
				const int nothing = 100;

				var transportMessage = GetTransportMessage(nothing);

				ServiceResult actual = target.Send(transportMessage);
				Assert.IsTrue(actual.Ok);
			}
		}



		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.OptionDbInMemory)]
		public void WhenAllEndPointsFailed_CanBeReInvoked(DbEngineType engineType)
		{
			MockConnectivityProvider connMgr;
			using (ServiceProxy target = GetTarget(out connMgr))
			{
				const int nothing = 150;
				var expected = GetTransportMessage(nothing);
				connMgr.ClientProxiesForComponent.Clear();
				const int itemsNum = 10;
				for (int i = 0; i < itemsNum; i++)
				{
					connMgr.ClientProxiesForComponent.Add(new MockEndPoint() {Fails = true});
				}

				ServiceResult actual = target.Send(expected);

				Assert.IsFalse(actual.Ok);

				((MockEndPoint) connMgr.ClientProxiesForComponent[itemsNum - 1]).Fails = false;

				actual = target.Send(expected);

				Assert.IsTrue(actual.Ok);
			}
		}

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.OptionDbInMemory)]
		public void WhenAllEndPointsFailed__ServiceResultError(DbEngineType engineType)
		{
			MockConnectivityProvider connMgr;
			using (ServiceProxy target = GetTarget(out connMgr))
			{
				const int nothing = 2100;
				var expected = GetTransportMessage(nothing);
				connMgr.ClientProxiesForComponent.Clear();
				const int itemsNum = 10;
				for (int i = 0; i < itemsNum; i++)
				{
					connMgr.ClientProxiesForComponent.Add(new MockEndPoint() {Fails = true});
				}

				ServiceResult actual = target.Send(expected);

				Assert.IsFalse(actual.Ok);
			}
		}

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.OptionDbInMemory)]
		public void WhenEndpointFailedTry_WithAllTheRestOfEndpoints(DbEngineType engineType)
		{
			MockConnectivityProvider connMgr;
			using (ServiceProxy target = GetTarget(out connMgr))
			{
				const int nothing = 155;
				var expected = GetTransportMessage(nothing);
				connMgr.ClientProxiesForComponent.Clear();
				const int itemsNum = 10;
				for (int i = 0; i < itemsNum; i++)
				{
					connMgr.ClientProxiesForComponent.Add(new MockEndPoint() {Fails = true});
				}

				((MockEndPoint) connMgr.ClientProxiesForComponent[itemsNum - 1]).Fails = false;

				ServiceResult actual = target.Send(expected);

				Assert.IsTrue(actual.Ok);
			}
		}

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.OptionDbInMemory)]
		public void WhenEndpointRaisesExceptionTry_WithAllTheRestOfEndpoints(DbEngineType engineType)
		{
			MockConnectivityProvider connMgr;
			using (ServiceProxy target = GetTarget(out connMgr))
			{
				const int nothing = 10055;
				var expected = GetTransportMessage(nothing);
				((MockEndPoint) connMgr.ClientProxiesForComponent[0]).RaisesException = true;
				ServiceResult actual = target.Send(expected);
				Assert.IsTrue(actual.Ok);
			}
		}

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.OptionDbInMemory)]
		public void WhenThereAreNotEndpointsItReturns_ServiceResultError(DbEngineType engineType)
		{
			using (ServiceProxy target = GetTarget(false))
			{
				const int nothing = 5100;
				var expected = GetTransportMessage(nothing);
				ServiceResult actual = target.Send(expected);
				Assert.IsFalse(actual.Ok);
			}
		}
	}
}