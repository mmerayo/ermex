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
using System.Linq;
using System.Threading;
using Common.Logging;
using Common.Logging.Simple;
using NUnit.Framework;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Entities.Entities;
using ermeX.Tests.Common;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Dummies;
using ermeX.Tests.Common.RandomValues;
using ermeX.Tests.Common.SettingsProviders;

using ermeX.Tests.WorldGateTests.Mock;
using ermeX.Tests.WorldGateTests.Mock.Messages;

namespace ermeX.Tests.WorldGateTests
{
	[Category(TestCategories.CoreSystemTest)]
	//[TestFixture]
	internal class UsingMessagesClientTests : DataAccessTestBase
	{
		#region Setup/Teardown

		public override void OnTearDown()
		{
			WorldGate.Reset();
			TestService.Reset();

			base.OnTearDown();
		}

		public override void OnStartUp()
		{
			CreateDatabase = false;
			base.OnStartUp();

			//TODO: REMOVE FROM HERE
			//LogManager.Adapter = new NoOpLoggerFactoryAdapter();
		}

		#endregion

		

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.DbInMemory)]
		public void Can_Receive_PublishedMessage(DbEngineType dbEngine)
		{
			var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
			WorldGate.ConfigureAndStart(cfg);

			var autoResetEvent = new AutoResetEvent(false);

			var handler = WorldGate.Suscribe<TestMessageHandlerOneMessage>(typeof (TestMessageHandlerOneMessage));
			handler.ExpectedMessages = 1;
			handler.SetReceivedEvent(autoResetEvent);
			var dummyDomainEntity = new DummyDomainEntity {Id = Guid.NewGuid()};
			WorldGate.Publish(dummyDomainEntity);

			autoResetEvent.WaitOne(new TimeSpan(0, 0, 25));
			var lastEntityReceived = handler.LastEntityReceived<DummyDomainEntity>();

			Assert.IsNotNull(lastEntityReceived);
			Assert.IsTrue(lastEntityReceived.Id == dummyDomainEntity.Id);
		}

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.DbInMemory)]
		public void Can_Receive_Several_Messages(DbEngineType dbEngine)
		{
			var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
			WorldGate.ConfigureAndStart(cfg);

			var autoResetEvent = new AutoResetEvent(false);


			var handler = WorldGate.Suscribe<TestMessageHandlerSeveralMessages>();
			TestMessageHandlerSeveralMessages.AutoResetEvent = autoResetEvent;
			handler.ExpectedMessages = 1;
			var dummyDomainEntity = new DummyDomainEntity {Id = Guid.NewGuid()};
			WorldGate.Publish(dummyDomainEntity);

			autoResetEvent.WaitOne(new TimeSpan(0, 0, 25));
			var lastEntityReceived = handler.LastEntityReceived<DummyDomainEntity>();

			Assert.IsNotNull(lastEntityReceived);
			Assert.IsTrue(lastEntityReceived.Id == dummyDomainEntity.Id);

			handler.Clear();
			handler.ExpectedMessages = 1;
			autoResetEvent.Reset();
			var dummyDomainEntity2 = new DummyDomainEntity2 {Data = RandomHelper.GetRandomString()};
			WorldGate.Publish(dummyDomainEntity2);

			autoResetEvent.WaitOne(new TimeSpan(0, 0, 25));
			var lastEntityReceived2 = handler.LastEntityReceived<DummyDomainEntity2>();

			Assert.IsNotNull(lastEntityReceived2);
			Assert.IsTrue(lastEntityReceived2.Data == dummyDomainEntity2.Data);
		}

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.DbInMemory)]
		public void BaseTypeHandler_Receives_Inherited_Message(DbEngineType dbEngine)
		{
			var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
			WorldGate.ConfigureAndStart(cfg);

			var autoResetEvent = new AutoResetEvent(false);

			var handler = WorldGate.Suscribe<TestMessageHandlerBaseType>();
			TestMessageHandlerBaseType.AutoResetEvent = autoResetEvent;
			handler.ExpectedMessages = 1;
			var dummyDomainEntity = new DummyDomainEntity3
				{
					Data = RandomHelper.GetRandomString(),
					DateTime = RandomHelper.GetRandomDateTime()
				};
			WorldGate.Publish(dummyDomainEntity);

			autoResetEvent.WaitOne(new TimeSpan(0, 0, 25));
			var lastEntityReceived = handler.LastEntityReceived<DummyDomainEntity3>();

			Assert.IsNotNull(lastEntityReceived);
			Assert.IsTrue(lastEntityReceived.Data == dummyDomainEntity.Data);
			Assert.IsTrue(lastEntityReceived.DateTime == dummyDomainEntity.DateTime);
		}

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.DbPersistent)]
		public void BaseTypeHandler_And_ConcreteHandlerType_Receives_Inherited_Message(DbEngineType dbEngine)
		{
			var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
			WorldGate.ConfigureAndStart(cfg);

			var autoResetEvent = new AutoResetEvent(false);

			var handler = WorldGate.Suscribe<TestMessageHandlerSeveralMessagesWithInheritance>();
			//this one is subscribed to two messages
			handler.SetReceivedEvent(autoResetEvent);
			handler.ExpectedMessages = 2;
			var dummyDomainEntity = new DummyDomainEntity3
				{
					Data = RandomHelper.GetRandomString(),
					DateTime = RandomHelper.GetRandomDateTime()
				};
			WorldGate.Publish(dummyDomainEntity);

			autoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
			var lastEntityReceived = handler.LastEntityReceived<DummyDomainEntity3>();

			Assert.IsNotNull(lastEntityReceived);
			Assert.IsTrue(lastEntityReceived.Data == dummyDomainEntity.Data);
			Assert.IsTrue(lastEntityReceived.DateTime == dummyDomainEntity.DateTime);

			Thread.Sleep(500); //we want to ensure that nothing else is delivered 

			Assert.IsTrue(handler.ReceivedMessages.Count == 2,
			              string.Format("Expected:2 Received: {0}", handler.ReceivedMessages.Count));
			var first = (DummyDomainEntity3) handler.ReceivedMessages[0];
			var second = (DummyDomainEntity3) handler.ReceivedMessages[1];
			Assert.IsTrue(first.Data == second.Data);
			Assert.IsTrue(first.DateTime == second.DateTime);
		}

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.DbInMemory)]
		public void InterfaceTypeHandler_And_ConcreteHandlerType_Receives_Inherited_Message(DbEngineType dbEngine)
		{
			var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
			WorldGate.ConfigureAndStart(cfg);

			var autoResetEvent = new AutoResetEvent(false);

			var handler = WorldGate.Suscribe<TestMessageHandlerSeveralMessagesWithInterfacesInheritance>();
			handler.SetReceivedEvent(autoResetEvent);
			handler.ExpectedMessages = 2;
			var dummyDomainEntity = new DummyDomainEntity3
				{
					Data = RandomHelper.GetRandomString(),
					DateTime = RandomHelper.GetRandomDateTime()
				};
			WorldGate.Publish(dummyDomainEntity);

			autoResetEvent.WaitOne(new TimeSpan(0, 1, 0));
			var lastEntityReceived = handler.LastEntityReceived<DummyDomainEntity3>();

			Assert.IsNotNull(lastEntityReceived);
			Assert.IsTrue(lastEntityReceived.Data == dummyDomainEntity.Data);
			Assert.IsTrue(lastEntityReceived.DateTime == dummyDomainEntity.DateTime);
			Thread.Sleep(500); //ensure no more messages are received
			Assert.IsTrue(handler.ReceivedMessages.Count == 2);
			var first = (DummyDomainEntity3) handler.ReceivedMessages[0];
			var second = (DummyDomainEntity3) handler.ReceivedMessages[1];
			Assert.IsTrue(first.Data == second.Data);
			Assert.IsTrue(first.DateTime == second.DateTime);
		}


		//todo: interfaces, abstract types etc
	}
}