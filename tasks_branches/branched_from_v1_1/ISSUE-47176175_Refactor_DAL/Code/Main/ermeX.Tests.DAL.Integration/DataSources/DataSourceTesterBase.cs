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
using System.Threading;
using NUnit.Framework;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces.Observer;
using ermeX.Entities.Base;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;

namespace ermeX.Tests.DAL.Integration.DataSources
{
	internal abstract class DataSourceTesterBase<TDataSource, TModel> : DataAccessTestBase
		where TDataSource : Repository<TModel>
		where TModel : ModelBase, new()
	{

		protected class TestDalObserver : IDalObserver<TModel>
		{
			public readonly List<Tuple<NotifiableDalAction, TModel>> Notifications =
				new List<Tuple<NotifiableDalAction, TModel>>();

			public void Notify(NotifiableDalAction action, TModel entity)
			{
				lock (Notifications)
					Notifications.Add(new Tuple<NotifiableDalAction, TModel>(action, entity));
			}
		}

		protected abstract string IdFieldName { get; }
		protected abstract string TableName { get; }


		[Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
		public void SetsVersion_When_New(DbEngineType engine)
		{
			DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
			dataAccessTestHelper.QueryTestHelper.ExecuteNonQuery(String.Format("Delete From {0}.{1}", SchemaName, TableName));

			var expected = GetExpected(engine);
			expected.ComponentOwner = LocalComponentId;
			Assert.IsTrue(expected.Version == DateTime.MinValue.Ticks, "implementation of GetExpected must set version to 0");

			var unitOfWorkFactory = GetUnitOfWorkFactory(engine);
			using (var uow = unitOfWorkFactory.Create())
			{
				var target = GetRepository<TDataSource>();
				target.Save(uow.Session, expected);
				uow.Commit();
			}

			Assert.IsTrue(expected.Version > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 10)).Ticks);

			Assert.IsTrue(expected.Version <= DateTime.UtcNow.Ticks);

			var actual = dataAccessTestHelper.QueryTestHelper.GetObjectFromRow<TModel>(GetByIdSqlQuery(expected));
			Assert.AreEqual(expected, actual);


		}


		[Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
		public void Updates_Version_When_Existing_With_Local(DbEngineType engine)
		{
			int id = InsertRecord(engine);
			var factory = GetUnitOfWorkFactory(engine);
			TModel expected;
			using (var uow = factory.Create())
			{
				var target = GetRepository<TDataSource>();
				expected = target.Single(uow.Session, id);
				uow.Commit();
			}
			Assert.IsTrue(expected.Version != DateTime.MinValue.Ticks);
			
			expected = GetExpectedWithChanges(expected);
			long expVersion = expected.Version;
			Thread.Sleep(50);
			using (var uow = factory.Create())
			{
				var target = GetRepository<TDataSource>();
				target.Save(uow.Session, expected);
				uow.Commit();
			}
			var dataAccessTestHelper = GetDataHelper(engine);
			var actual = dataAccessTestHelper.QueryTestHelper.GetObjectFromRow<TModel>(GetByIdSqlQuery(expected));

			Assert.AreNotEqual(expVersion, actual.Version);
			Assert.IsTrue(expVersion < actual.Version);
		}


		[Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
		public void DontChange_Version_When_IsFromOtherComponent(DbEngineType engine)
		{
			TModel item;
			TModel expected;
			int id = InsertRecord(engine);
			var factory = GetUnitOfWorkFactory(engine);
			using (var uow = factory.Create())
			{
				var target = GetRepository<TDataSource>();
				item = target.Single(uow.Session, id);

				Assert.IsTrue(item.Version != DateTime.MinValue.Ticks);
				expected = GetExpectedWithChanges(item);
				expected.ComponentOwner = Guid.NewGuid();
				target.Save(uow.Session, expected);
				uow.Commit();
			}
			DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
			var actual = dataAccessTestHelper.QueryTestHelper.GetObjectFromRow<TModel>(GetByIdSqlQuery(expected));
			Assert.AreEqual(expected.Version, actual.Version);
		}


		//TODO: REMOVE COMMENT [Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
		[Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
		public void CanAddRecord(DbEngineType engine)
		{
			DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
			dataAccessTestHelper.QueryTestHelper.ExecuteNonQuery(String.Format("Delete From {0}.{1}", SchemaName, TableName));

			TModel expected = GetExpected(engine);
			var unitOfWorkFactory = GetUnitOfWorkFactory(engine);
			using (var uow = unitOfWorkFactory.Create())
			{
				var target = GetRepository<TDataSource>();
				target.Save(uow.Session, expected);
				uow.Commit();
			}
			Assert.IsTrue(expected.Id > 0);

			var actual = GetObjectFromRow(engine, expected);
			Assert.AreEqual(expected, actual);
		}

		protected virtual TModel GetObjectFromRow(DbEngineType enginetype, TModel expected)
		{
			return GetDataHelper(enginetype).QueryTestHelper.GetObjectFromRow<TModel>(GetByIdSqlQuery(expected));
		}

		[Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
		public void CanUpdateRecord(DbEngineType engine)
		{
			int id = InsertRecord(engine);
			TModel expected;
			IUnitOfWorkFactory unitOfWorkFactory = GetUnitOfWorkFactory(engine);
			using (var uow = unitOfWorkFactory.Create())
			{
				var target = GetRepository<TDataSource>();
				TModel item = target.Single(uow.Session, id);
				expected = GetExpectedWithChanges(item);
				target.Save(uow.Session, expected);
				uow.Commit();
			}
			var dataAccessTestHelper = GetDataHelper(engine);
			var actual = dataAccessTestHelper.QueryTestHelper.GetObjectFromRow<TModel>(GetByIdSqlQuery(expected));

			Assert.AreEqual(expected, actual);
		}

		protected abstract TModel GetExpectedWithChanges(TModel source);

		protected string GetByIdSqlQuery(TModel expected)
		{
			return string.Format("Select * from {3}.{0} where {1}={2}", TableName, IdFieldName, expected.Id, SchemaName);
		}

		[Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
		public void CanDeleteByProperty(DbEngineType engine)
		{
			int id = InsertRecord(engine);
			DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
			var numRecords =
				dataAccessTestHelper.QueryTestHelper.ExecuteScalar<int>(string.Format("Select count(*) from {3}.{0} where {1}={2}",
																					  TableName, IdFieldName, id, SchemaName));
			Assert.IsTrue(numRecords == 1);
			IUnitOfWorkFactory unitOfWorkFactory = GetUnitOfWorkFactory(engine);
			using (var uow = unitOfWorkFactory.Create())
			{
				var target = GetRepository<TDataSource>();
				target.Remove(uow.Session, x => x.Id == id);
				uow.Commit();
			}
			numRecords =
				dataAccessTestHelper.QueryTestHelper.ExecuteScalar<int>(string.Format("Select count(*) from {3}.{0} where {1}={2}",
																					  TableName, IdFieldName, id, SchemaName));
			Assert.IsTrue(numRecords == 0);
		}


		[Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
		public void CanDelete(DbEngineType engine)
		{
			int id = InsertRecord(engine);

			IUnitOfWorkFactory unitOfWorkFactory = GetUnitOfWorkFactory(engine);
			using (var uow = unitOfWorkFactory.Create())
			{
				var target = GetRepository<TDataSource>();
				TModel actual = target.Single(uow.Session, id);
				Assert.IsNotNull(actual);
				target.Remove(uow.Session, actual);
				uow.Commit();
			}
			DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
			var numRecords =
				dataAccessTestHelper.QueryTestHelper.ExecuteScalar<int>(string.Format("Select count(*) from {3}.{0} where {1}={2}",
																					  TableName, IdFieldName, id, SchemaName));
			Assert.IsTrue(numRecords == 0);
		}

		[Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
		public void CanGetById(DbEngineType engine)
		{
			int id = InsertRecord(engine);

			TModel actual;
			IUnitOfWorkFactory unitOfWorkFactory = GetUnitOfWorkFactory(engine);
			using (var uow = unitOfWorkFactory.Create())
			{
				var target = GetRepository<TDataSource>();
				actual = target.Single(uow.Session, id);
				Assert.IsNotNull(actual);
				uow.Commit();
			}

			CheckInsertedRecord(actual);
		}

		protected abstract int InsertRecord(DbEngineType engine);

		protected abstract void CheckInsertedRecord(TModel record);

		protected abstract TModel GetExpected(DbEngineType engine);

	}
}