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
using Common.Logging;
using Common.Logging.Simple;
using NUnit.Framework;
using ermeX.Common;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.Domain.Component;
using ermeX.Domain.Implementations.Component;
using ermeX.Domain.Implementations.Observers;
using ermeX.Domain.Implementations.QueryDatabase;
using ermeX.Domain.Implementations.Queues;
using ermeX.Domain.Implementations.Subscriptions;
using ermeX.Domain.Observers;
using ermeX.Domain.QueryDatabase;
using ermeX.Domain.Queues;
using ermeX.Domain.Subscriptions;
using ermeX.NonMerged;

namespace ermeX.Tests.Common.DataAccess
{
	[TestFixture]
	internal abstract class DataAccessTestBase
	{
		#region infrastructure

		protected readonly Guid LocalComponentId = Guid.NewGuid();
		protected readonly Guid RemoteComponentId = Guid.NewGuid();

		#region Datahelper

		protected const string SchemaName = "[ClientComponent]";
		private readonly object _syncLock = new object();

		private readonly Dictionary<DbEngineType, DataAccessTestHelper> _dataHelpers =
			new Dictionary<DbEngineType, DataAccessTestHelper>(Enum.GetValues(typeof (DbEngineType)).Length);

		protected DataAccessTestHelper GetDataHelper(DbEngineType engineType)
		{
			if (!_dataHelpers.ContainsKey(engineType))
				lock (_syncLock)
					if (!_dataHelpers.ContainsKey(engineType))
						_dataHelpers.Add(engineType,
						                 new DataAccessTestHelper(engineType, CreateDatabase, SchemaName, LocalComponentId,
						                                          RemoteComponentId));

			return _dataHelpers[engineType];
		}

		#endregion

		#endregion

		private bool _createDatabase = true;

		protected IQueryHelperFactory QueryHelperFactory
		{
			get { return new QueryHelperFactory(); }
		}

		protected virtual List<DataSchemaType> SchemasToApply
		{
			get
			{
				return new List<DataSchemaType>
					{
						DataSchemaType.ClientComponent
					};
			}
		}

		/// <summary>
		/// specifies if the test db provider creates a new database
		/// </summary>
		protected bool CreateDatabase
		{
			get { return _createDatabase; }
			set { _createDatabase = value; }
		}

		[TestFixtureTearDown]
		public virtual void OnFixtureTearDown()
		{
			foreach (var value in _dataHelpers.Values)
			{
				value.Dispose();
			}
		}

		[TearDown]
		public virtual void OnTearDown()
		{
			foreach (var value in _dataHelpers.Values)
			{
				value.ClearData();
			}
		}

		private DataSourcesFactory _dataSourcesFactory = null;

		[SetUp]
		public virtual void OnStartUp()
		{
			if (LogManager.Adapter is NoOpLoggerFactoryAdapter)
				LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(LogLevel.All, true, true, true, "yyyy/MM/dd HH:mm:ss:fff");
		}

		[TestFixtureSetUp]
		public virtual void OnFixtureSetup()
		{
			ResolveUnmerged.Init();
			_dataSourcesFactory = new DataSourcesFactory();
		}

		private readonly Dictionary<DbEngineType, DataAccessExecutor> _dataAccessExecutors =
			new Dictionary<DbEngineType, DataAccessExecutor>();

		protected DataAccessExecutor GetdataAccessExecutor(DbEngineType engineType)
		{
			if (!_dataAccessExecutors.ContainsKey(engineType))
			{
				var dataAccessExecutor = new DataAccessExecutor(GetDataHelper(engineType).DataAccessSettings);
				_dataAccessExecutors.Add(engineType, dataAccessExecutor);
			}
			return _dataAccessExecutors[engineType];
		}

		protected TResult GetDataSource<TResult>(DbEngineType engineType)
		{
			return _dataSourcesFactory.GetDataSource<TResult>(engineType, GetdataAccessExecutor(engineType),
			                                                  LocalComponentId);
		}

		private class DataSourcesFactory
		{
			private readonly Dictionary<DbEngineType, Dictionary<Type, object>> _dataSourcesCache =
				new Dictionary<DbEngineType, Dictionary<Type, object>>();

			public TResult GetDataSource<TResult>(DbEngineType engineType, DataAccessExecutor executor, Guid componentOwner)
			{
				if (!_dataSourcesCache.ContainsKey(engineType))
					_dataSourcesCache.Add(engineType, new Dictionary<Type, object>());

				var dictionary = _dataSourcesCache[engineType];

				if (!dictionary.ContainsKey(typeof (TResult)))
				{
					var fromType = ObjectBuilder.FromType<TResult>(typeof (TResult), executor.DalSettings, componentOwner, executor);
					dictionary.Add(typeof (TResult), fromType);
				}
				return (TResult) dictionary[typeof (TResult)];
			}
		}

		protected IWriteIncommingQueue GetIncommingQueueWritter(DbEngineType dbEngine)
		{
			var dataSource = GetDataSource<IncomingMessagesDataSource>(dbEngine);
			return new IncommingQueueWriter(dataSource);
		}

		protected IReadIncommingQueue GetIncommingQueueReader(DbEngineType dbEngine)
		{
			var dataSource = GetDataSource<IncomingMessagesDataSource>(dbEngine);
			return new ReaderIncommingQueue(dataSource);
		}

		protected ICanUpdateLatency GetLatenciesWritter(DbEngineType dbEngine)
		{
			return new LatencyUpdater(GetDataSource<AppComponentDataSource>(dbEngine));
		}

		protected ICanReadLatency GetLatenciesReader(DbEngineType dbEngine)
		{
			return new LatencyReader(GetDataSource<AppComponentDataSource>(dbEngine));
		}

		protected ICanReadIncommingMessagesSubscriptions GetIncommingMessageSubscriptionsReader(DbEngineType dbEngine)
		{
			return new CanReadIncommingMessagesSubscriptions(GetDataSource<IncomingMessageSuscriptionsDataSource>(dbEngine));
		}

		protected ICanReadOutgoingMessagesSubscriptions GetOutgoingMessageSubscriptionsReader(DbEngineType dbEngine)
		{
			return new CanReadOutgoingMessagesSubscriptions(GetDataSource<OutgoingMessageSuscriptionsDataSource>(dbEngine));
		}

		protected IWriteOutgoingQueue GetOutgoingMessageSubscriptionsWritter(DbEngineType dbEngine)
		{
			return new WriteOutgoingQueue(GetDataSource<OutgoingMessagesDataSource>(dbEngine));
		}

		protected IDomainObservable GetDomainNotifier(DbEngineType dbEngine)
		{
			return new DomainNotifier(GetDataSource<OutgoingMessageSuscriptionsDataSource>(dbEngine));
		}

		protected IWriteOutgoingQueue GetOutgoingQueueWritter(DbEngineType dbEngine)
		{
			return new WriteOutgoingQueue(GetDataSource<OutgoingMessagesDataSource>(dbEngine));
		}

		protected IReadOutgoingQueue GetOutgoingQueueReader(DbEngineType dbEngine)
		{
			return new ReaderOutgoingQueue(GetDataSource<OutgoingMessagesDataSource>(dbEngine));
		}
	}
}