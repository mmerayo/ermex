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
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.DAL.Commands.Component;
using ermeX.DAL.Commands.QueryDatabase;
using ermeX.DAL.Commands.Queues;
using ermeX.DAL.Commands.Subscriptions;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.DAL.DataAccess.Providers;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Interfaces.QueryDatabase;
using ermeX.DAL.Interfaces.Queues;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.Entities.Entities;
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
			new Dictionary<DbEngineType, DataAccessTestHelper>(Enum.GetValues(typeof(DbEngineType)).Length);

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

		//TODO: MOVE FROM HERE, CLEAN,...
		public class CompSettings:IComponentSettings
		{
			public Guid ComponentId { get; set; }
			public int CacheExpirationSeconds { get; private set; }
			public Type ConfigurationManagerType { get; private set; }
			public bool DevLoggingActive { get; private set; }
		}

		protected IComponentSettings GetComponentSettings()
		{
				return new CompSettings{ComponentId = LocalComponentId};
		}

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

		private readonly Dictionary<DbEngineType, ISessionProvider> _sessionProviders =
			new Dictionary<DbEngineType, ISessionProvider>();

		public IUnitOfWorkFactory GetUnitOfWorkFactory(IDalSettings dalSettings)
		{
			if (!_sessionProviders.ContainsKey(dalSettings.ConfigurationSourceType))
				lock (_sessionProviders)
					if (!_sessionProviders.ContainsKey(dalSettings.ConfigurationSourceType))
						_sessionProviders.Add(dalSettings.ConfigurationSourceType, new SessionProvider(dalSettings));

			return new UnitOfWorkFactory(_sessionProviders[dalSettings.ConfigurationSourceType]);
		}
		public IUnitOfWorkFactory GetUnitOfWorkFactory(DbEngineType dbEngineType)
		{
			var dataAccessSettings = GetDataHelper(dbEngineType).DataAccessSettings;
			return GetUnitOfWorkFactory(dataAccessSettings);
		}

		//public TResult GetRepository<TResult>(IDalSettings dalSettings)
		//{
		//    var unitOfWorkFactory = GetUnitOfWorkFactory(dalSettings);
		//    return GetRepository<TResult>(unitOfWorkFactory);
		//}

		public TResult GetRepository<TResult>(IUnitOfWorkFactory factory)
		{
			if (factory == null) throw new ArgumentNullException("factory");


			var result = ObjectBuilder.FromType<TResult>(typeof(TResult), GetComponentSettings(), factory, new ExpressionsHelper());
			return result;
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
		}

		protected IDalSettings GetDalSettings(DbEngineType engineType)
		{
			return GetDataHelper(engineType).DataAccessSettings;
		}

		//TODO: REMOVE THIS AS IT NEEDS THE FACTORY CREATED AND USE THE OVERLOAD, SEE TEST BASE
		//protected TResult GetRepository<TResult>(DbEngineType engineType)
		//{
		//    return GetRepository<TResult>(GetDataHelper(engineType).DataAccessSettings);
		//}

		protected IWriteIncommingQueue GetIncommingQueueWritter(DbEngineType dbEngine)
		{
			var dataSource = GetRepository<Repository<IncomingMessage>>(dbEngine);
			var dalSettings = GetDalSettings(dbEngine);
			var unitOfWorkFactory = GetUnitOfWorkFactory(dalSettings);
			var componentSettings = GetComponentSettings();
			return new IncommingQueueWriter(dataSource,unitOfWorkFactory,componentSettings);
		}

		protected IReadIncommingQueue GetIncommingQueueReader(DbEngineType dbEngine)
		{
			var dataSource = GetRepository<Repository<IncomingMessage>>(dbEngine);
			var dalSettings = GetDalSettings(dbEngine);
			var unitOfWorkFactory = GetUnitOfWorkFactory(dalSettings);
			var componentSettings = GetComponentSettings();
			return new ReaderIncommingQueue(dataSource, unitOfWorkFactory, componentSettings);
		}

		protected ICanUpdateLatency GetLatenciesWritter(DbEngineType dbEngine)
		{
			var dataSource = GetRepository<Repository<AppComponent>>(dbEngine);
			var dalSettings = GetDalSettings(dbEngine);
			var unitOfWorkFactory = GetUnitOfWorkFactory(dalSettings);
			return new LatencyUpdater(dataSource, unitOfWorkFactory);
		}

		protected ICanReadLatency GetLatenciesReader(DbEngineType dbEngine)
		{
			var dataSource = GetRepository<Repository<AppComponent>>(dbEngine);
			var dalSettings = GetDalSettings(dbEngine);
			var unitOfWorkFactory = GetUnitOfWorkFactory(dalSettings);

			return new LatencyReader(dataSource,unitOfWorkFactory);
		}

		protected ICanReadIncommingMessagesSubscriptions GetIncommingMessageSubscriptionsReader(DbEngineType dbEngine)
		{
			var dataSource = GetRepository<Repository<IncomingMessageSuscription>>(dbEngine);
			var dalSettings = GetDalSettings(dbEngine);
			var unitOfWorkFactory = GetUnitOfWorkFactory(dalSettings);
			var componentSettings = GetComponentSettings();

			return new CanReadIncommingMessagesSubscriptions(dataSource,unitOfWorkFactory,componentSettings);
		}

		protected ICanReadOutgoingMessagesSubscriptions GetOutgoingMessageSubscriptionsReader(DbEngineType dbEngine)
		{
			var dataSource = GetRepository<Repository<OutgoingMessageSuscription>>(dbEngine);
			var dalSettings = GetDalSettings(dbEngine);
			var unitOfWorkFactory = GetUnitOfWorkFactory(dalSettings);
			var componentSettings = GetComponentSettings();
			return new CanReadOutgoingMessagesSubscriptions(dataSource,unitOfWorkFactory,componentSettings);
		}

		//protected ICanUpdateOutgoingMessagesSubscriptions GetOutgoingMessageSubscriptionsWritter(DbEngineType dbEngine)
		//{
		//    var dataSource = GetRepository<Repository<OutgoingMessageSuscription>>(dbEngine);
		//    var dalSettings = GetDalSettings(dbEngine);
		//    var unitOfWorkFactory = _dataSourcesFactory.GetUnitOfWorkFactory(dalSettings);
		//    var componentSettings = GetComponentSettings();

		//    return new CanUpdateOutgoingMessagesSubscriptions(dataSource,);
		//}

		//protected IDomainObservable GetDomainNotifier(DbEngineType dbEngine)
		//{
		//    return new DomainNotifier(GetDataSource<OutgoingMessageSuscriptionsDataSource>(dbEngine));
		//}

		protected IWriteOutgoingQueue GetOutgoingQueueWritter(DbEngineType dbEngine)
		{
			var dataSource = GetRepository<Repository<OutgoingMessage>>(dbEngine);
			var dalSettings = GetDalSettings(dbEngine);
			var unitOfWorkFactory = GetUnitOfWorkFactory(dalSettings);
			var componentSettings = GetComponentSettings();

			return new WriteOutgoingQueue(dataSource,unitOfWorkFactory,componentSettings);
		}

		protected IReadOutgoingQueue GetOutgoingQueueReader(DbEngineType dbEngine)
		{
			var dataSource = GetRepository<Repository<OutgoingMessage>>(dbEngine);
			var dalSettings = GetDalSettings(dbEngine);
			var unitOfWorkFactory = GetUnitOfWorkFactory(dalSettings);
			var componentSettings = GetComponentSettings();

			return new ReaderOutgoingQueue(dataSource,unitOfWorkFactory,componentSettings);
		}

	}
}