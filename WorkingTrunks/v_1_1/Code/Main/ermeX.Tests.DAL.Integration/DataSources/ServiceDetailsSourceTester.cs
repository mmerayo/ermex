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
using ermeX.Tests.Common.SettingsProviders;

namespace ermeX.Tests.DAL.Integration.DataSources
{
	//TODO: test al possible operations in all datasources

	//[TestFixture]
	internal class ServiceDetailsSourceTester :
		UpdatableByExternalComponentsTester<Repository<ServiceDetails>, ServiceDetails>
	{
		protected override string IdFieldName
		{
			get { return ServiceDetails.GetDbFieldName("Id"); }
		}

		protected override string TableName
		{
			get { return ServiceDetails.TableName; }
		}

		protected override ServiceDetails GetExpectedWithChanges(ServiceDetails source)
		{
			source.ServiceImplementationTypeName = "GetExpectedWithChangesForTestType";
			return source;
		}


		protected override int InsertRecord(DbEngineType engine)
		{

			return GetDataHelper(engine).InsertServiceDetails(LocalComponentId, RemoteComponentId, serviceTestMethodName,
			                                                  serviceTestTypeName, serviceTestInterfaceName, operationId,
			                                                  DateTime.UtcNow, false);
		}

		protected override void CheckInsertedRecord(ServiceDetails record)
		{
			Assert.IsNotNull(record);
			Assert.AreEqual(operationId, record.OperationIdentifier);
			Assert.AreEqual(serviceTestTypeName, record.ServiceImplementationTypeName);
			Assert.AreEqual(serviceTestMethodName, record.ServiceImplementationMethodName);
			Assert.AreEqual(RemoteComponentId, record.Publisher);
		}

		protected override ServiceDetails GetExpected(DbEngineType engine)
		{
			return new ServiceDetails
				{
					ComponentOwner = OwnerComponentId,
					OperationIdentifier = operationId,
					ServiceImplementationTypeName = serviceTestTypeName,
					ServiceImplementationMethodName = serviceTestMethodName,
					ServiceInterfaceTypeName = serviceTestInterfaceName,
					Publisher = RemoteComponentId
				};
		}

		private Guid OwnerComponentId
		{
			get { return LocalComponentId; }
		}

		private readonly Guid operationId = Guid.NewGuid();
		private string serviceTestTypeName = "TestTypeName";
		private string serviceTestMethodName = "TestMethodName";
		private string serviceTestInterfaceName = "TestInterfaceName";

		//[Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
		//public void CanGetByOperationId(DbEngineType engine)
		//{
		//    int id = InsertRecord(engine);

		//    ServiceDetailsDataSource target = GetDataSource<ServiceDetailsDataSource>(engine);
		//    ServiceDetails actual = target.GetByOperationId(RemoteComponentId, operationId);
		//    Assert.IsNotNull(actual);

		//    CheckInsertedRecord(actual);
		//}

		//[Ignore("Decide if this test is still useful as it needs to be modified")]
		//[Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
		//public void CantInsertRepeatedOperations(DbEngineType engine)
		//{
		//    ServiceDetails serviceDetails1 = GetExpected(engine);

		//    ServiceDetailsDataSource target = GetDataSource<ServiceDetailsDataSource>(engine);
		//    target.Save(serviceDetails1);

		//    ServiceDetails serviceDetails2 = GetExpected(engine);
		//    serviceDetails2.OperationIdentifier = Guid.NewGuid();
		//    serviceDetails2.Version = DateTime.UtcNow.Ticks;
		//    Assert.Throws<InvalidOperationException>(() => target.Save(serviceDetails2));
		//}
	}
}