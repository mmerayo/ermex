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
using NUnit.Framework;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.Repository;
using ermeX.Entities.Entities;
using ermeX.Tests.Common.DataAccess;

namespace ermeX.Tests.DAL.Integration.DataSources
{
	//[TestFixture]
	internal class ConnectivityDetailsDataSourceTester :
		UpdatableByExternalComponentsTester<Repository<ConnectivityDetails>, ConnectivityDetails>
	{
		protected override string IdFieldName
		{
			get { return ConnectivityDetails.GetDbFieldName("Id"); }
		}

		protected override string TableName
		{
			get { return string.Format("[{0}]", ConnectivityDetails.TableName); }
		}



		private readonly Guid serverId = Guid.NewGuid();

		protected override int InsertRecord(DbEngineType engine)
		{
			Guid cid;
			DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
			dataAccessTestHelper.InsertAppComponent(ComponentId, ComponentOwnerId, 0, false, false);
			return dataAccessTestHelper.InsertConnectivityDetailsRecord(ComponentId, ComponentOwnerId, IP, Port, false, serverId,
			                                                            DateTime.UtcNow);
		}

		protected override void CheckInsertedRecord(ConnectivityDetails record)
		{
			Assert.IsNotNull(record);
			Assert.AreEqual(record.Ip, IP);
			Assert.AreEqual(record.Port, Port);
		}

		protected override ConnectivityDetails GetExpected(DbEngineType engine)
		{
			DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
			int idComponent = dataAccessTestHelper.InsertAppComponent(Guid.NewGuid(), LocalComponentId, 0, false, false);
			AppComponent appComponent;
			var unitOfWorkFactory = GetUnitOfWorkFactory(engine);

			var repository = GetRepository<Repository<AppComponent>>(unitOfWorkFactory);
			appComponent = repository.Single(idComponent);

			return new ConnectivityDetails
			       	{
			       		ComponentOwner = appComponent.ComponentOwner,
			       		Ip = IP,
			       		Port = Port,
			       		ServerId = appComponent.ComponentId
			       	};
		}


		protected override ConnectivityDetails GetExpectedWithChanges(ConnectivityDetails source)
		{
			source.Ip = IP2;
			return source;
		}

		private readonly Guid ComponentId = Guid.NewGuid();

		private Guid ComponentOwnerId
		{
			get { return LocalComponentId; }
		}

		private const string IP = "111.222.333.123";
		private const string IP2 = "222.222.333.123";
		private const int Port = 6666;
	}
}