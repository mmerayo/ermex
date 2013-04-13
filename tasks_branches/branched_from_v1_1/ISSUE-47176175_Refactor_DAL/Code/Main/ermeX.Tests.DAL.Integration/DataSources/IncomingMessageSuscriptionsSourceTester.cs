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
using ermeX.DAL.DataAccess.Helpers;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.Entities.Entities;
using ermeX.Tests.Common.DataAccess;

namespace ermeX.Tests.DAL.Integration.DataSources
{
	internal class IncomingMessageSuscriptionsSourceTester :
		DataSourceTesterBase<Repository<IncomingMessageSuscription>, IncomingMessageSuscription>
	{
		private static readonly Guid _suscriptionHandlerId = Guid.NewGuid();
		private static readonly DateTime _updateTime = new DateTime(2012, 2, 5, 7, 8, 9, 330);
		private string _messageType = "Test.Type";
		private const string _handlerType = "this is a type";


		protected override string IdFieldName
		{
			get { return IncomingMessageSuscription.GetDbFieldName("Id"); }
		}

		protected override string TableName
		{
			get { return IncomingMessageSuscription.TableName; }
		}

		protected override IncomingMessageSuscription GetExpectedWithChanges(IncomingMessageSuscription source)
		{
			source.BizMessageFullTypeName = "another";
			return source;
		}

		protected override int InsertRecord(DbEngineType engine)
		{
			Guid cid;
			DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
			dataAccessTestHelper.InsertAppComponent(LocalComponentId, LocalComponentId, 0, false, false);

			return dataAccessTestHelper.InsertIncomingMessageSuscriptions(_messageType, _updateTime, LocalComponentId,
			                                                              _suscriptionHandlerId, _handlerType);
		}


		protected override void CheckInsertedRecord(IncomingMessageSuscription record)
		{
			Assert.IsNotNull(record);
			Assert.IsTrue(record.SuscriptionHandlerId == _suscriptionHandlerId);
			Assert.AreEqual(_handlerType, record.HandlerType);
#if !NEED_FIX_MILLISECONDS
			Assert.IsTrue(record.DateLastUpdateUtc == _updateTime); //TODO:milliseconds are truncated
#endif
			Assert.AreEqual(_messageType, record.BizMessageFullTypeName);
		}

		protected override IncomingMessageSuscription GetExpected(DbEngineType engine)
		{
			DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
			int idComponent = dataAccessTestHelper.InsertAppComponent(Guid.NewGuid(), LocalComponentId, 0, false, false);
			AppComponent appComponent;
			var repository = GetRepository<Repository<AppComponent>>(engine);
			appComponent = repository.Single(idComponent);


			return new IncomingMessageSuscription
			       	{
			       		ComponentOwner = appComponent.ComponentId,
			       		BizMessageFullTypeName = _messageType,
			       		DateLastUpdateUtc = _updateTime,
			       		SuscriptionHandlerId = _suscriptionHandlerId,
			       		HandlerType = _handlerType
			       	};
		}
	}
}