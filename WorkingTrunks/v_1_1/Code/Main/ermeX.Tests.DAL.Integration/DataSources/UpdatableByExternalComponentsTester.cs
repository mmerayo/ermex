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
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Entities.Base;
using ermeX.Tests.Common.SettingsProviders;

namespace ermeX.Tests.DAL.Integration.DataSources
{
	/// <summary>
	/// inherit from this if the entity can be updated by external components
	/// </summary>
	/// <typeparam name="TDataSource"></typeparam>
	/// <typeparam name="TModel"></typeparam>
	internal abstract class UpdatableByExternalComponentsTester<TDataSource, TModel> :
		DataSourceTesterBase<TDataSource, TModel>
		where TDataSource : Repository<TModel>
		where TModel : ModelBase, new()
	{
		[Test, TestCaseSource(typeof (TestCaseSources), "AllDbs")]
		public void Doesnt_Update_When_Version_Is_Older_Than_Current(DbEngineType engine)
		{
			int id = InsertRecord(engine);
			TModel expected;
			IUnitOfWorkFactory unitOfWorkFactory = GetUnitOfWorkFactory(engine);
			long expVersion;

			using (var uow = unitOfWorkFactory.Create(false))
			{
				var target = GetRepository<TDataSource>(unitOfWorkFactory);
				expected = target.Single(uow, id);

				Assert.IsTrue(expected.Version != DateTime.MinValue.Ticks);

				expected = GetExpectedWithChanges(expected);

				target.Save(uow, expected);
				uow.Commit();
			}
			//another unit of work
			using (var uow = unitOfWorkFactory.Create(false))
			{
				var target = GetRepository<TDataSource>(unitOfWorkFactory);
				expVersion = expected.Version;
				expected.Version--;

				target.Save(uow, expected);
				uow.Commit();
			}
			
			var actual = GetDataHelper(engine).QueryTestHelper.GetObjectFromRow<TModel>(GetByIdSqlQuery(expected));

			Assert.AreEqual(expVersion.ToString(), actual.Version.ToString());
		}
	}
}