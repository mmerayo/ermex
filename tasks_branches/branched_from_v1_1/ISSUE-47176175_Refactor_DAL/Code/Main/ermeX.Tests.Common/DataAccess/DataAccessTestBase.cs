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
using ermeX.DAL.Commands.Messages;
using ermeX.DAL.Commands.Observers;
using ermeX.DAL.Commands.QueryDatabase;
using ermeX.DAL.Commands.Queues;
using ermeX.DAL.Commands.Services;
using ermeX.DAL.Commands.Subscriptions;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.DAL.Providers;
using ermeX.DAL.Repository;
using ermeX.DAL.Transactions;
using ermeX.DAL.UnitOfWork;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Interfaces.Messages;
using ermeX.DAL.Interfaces.Observers;
using ermeX.DAL.Interfaces.QueryDatabase;
using ermeX.DAL.Interfaces.Queues;
using ermeX.DAL.Interfaces.Services;
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

		private readonly Dictionary<DbEngineType, IReadTransactionProvider> _readTransactionProviders =
			new Dictionary<DbEngineType, IReadTransactionProvider>();
		private readonly Dictionary<DbEngineType, IWriteTransactionProvider> _writeTransactionProviders =
			new Dictionary<DbEngineType, IWriteTransactionProvider>();
		public IUnitOfWorkFactory GetUnitOfWorkFactory(IDalSettings dalSettings)
		{
			if (!_sessionProviders.ContainsKey(dalSettings.ConfigurationSourceType))
				lock (_sessionProviders)
					if (!_sessionProviders.ContainsKey(dalSettings.ConfigurationSourceType))
					{
						_sessionProviders.Add(dalSettings.ConfigurationSourceType, new SessionProvider(dalSettings));
						switch(dalSettings.ConfigurationSourceType)
						{
							case DbEngineType.SqlServer2008:
								var genericTransactionProvider = new GenericTransactionProvider();
								_readTransactionProviders.Add(dalSettings.ConfigurationSourceType,genericTransactionProvider);
								_writeTransactionProviders.Add(dalSettings.ConfigurationSourceType, genericTransactionProvider);
								break;
							case DbEngineType.Sqlite:
							case DbEngineType.SqliteInMemory:
								_readTransactionProviders.Add(dalSettings.ConfigurationSourceType, new GenericTransactionProvider());
								_writeTransactionProviders.Add(dalSettings.ConfigurationSourceType, new MutexedTransactionProvider(dalSettings));
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}


			return new UnitOfWorkFactory(_sessionProviders[dalSettings.ConfigurationSourceType], dalSettings,
			                             _readTransactionProviders[dalSettings.ConfigurationSourceType],
			                             _writeTransactionProviders[dalSettings.ConfigurationSourceType]);
		}
		public IUnitOfWorkFactory GetUnitOfWorkFactory(DbEngineType dbEngineType)
		{
			var dataAccessSettings = GetDataHelper(dbEngineType).DataAccessSettings;
			return GetUnitOfWorkFactory(dataAccessSettings);
		}

		/// <summary>
		/// Repository for complex operations
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="factory"></param>
		/// <returns></returns>
		public TResult GetRepository<TResult>(IUnitOfWorkFactory factory)
		{
			if (factory == null) throw new ArgumentNullException("factory");
			var result = ObjectBuilder.FromType<TResult>(typeof(TResult), GetComponentSettings(), new ExpressionsHelper(),factory);
			return result;
		}

		/// <summary>
		/// Repository for single db operations  
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="engineType"></param>
		/// <returns></returns>
		public TResult GetRepository<TResult>(DbEngineType engineType)
		{
			var unitOfWorkFactory = GetUnitOfWorkFactory(engineType);
			var result = GetRepository<TResult>(unitOfWorkFactory);
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

		protected IWriteIncommingQueue GetIncommingQueueWritter(IUnitOfWorkFactory factory)
		{
			var dataSource = GetRepository<Repository<IncomingMessage>>(factory);
			var componentSettings = GetComponentSettings();
			return new IncommingQueueWriter(dataSource,factory,componentSettings);
		}

		protected IReadIncommingQueue GetIncommingQueueReader(IUnitOfWorkFactory factory)
		{
			var dataSource = GetRepository<Repository<IncomingMessage>>(factory);
			var componentSettings = GetComponentSettings();
			return new ReaderIncommingQueue(dataSource, factory, componentSettings);
		}

		protected ICanUpdateLatency GetLatenciesWritter(IUnitOfWorkFactory factory)
		{
			var dataSource = GetRepository<Repository<AppComponent>>(factory);
			return new LatencyUpdater(dataSource, factory);
		}

		protected ICanReadLatency GetLatenciesReader(IUnitOfWorkFactory factory)
		{
			var dataSource = GetRepository<Repository<AppComponent>>(factory);
			return new LatencyReader(dataSource,factory);
		}

		protected ICanReadIncommingMessagesSubscriptions GetIncommingMessageSubscriptionsReader(IUnitOfWorkFactory factory)
		{
			var dataSource = GetRepository<Repository<IncomingMessageSuscription>>(factory);
			var componentSettings = GetComponentSettings();

			return new CanReadIncommingMessagesSubscriptions(dataSource,factory,componentSettings);
		}

		protected ICanReadOutgoingMessagesSubscriptions GetOutgoingMessageSubscriptionsReader(IUnitOfWorkFactory factory)
		{
			var dataSource = GetRepository<Repository<OutgoingMessageSuscription>>(factory);
			var componentSettings = GetComponentSettings();
			return new CanReadOutgoingMessagesSubscriptions(dataSource,factory,componentSettings);
		}

		protected ICanUpdateOutgoingMessagesSubscriptions GetOutgoingMessageSubscriptionsWritter(IUnitOfWorkFactory factory)
		{
			var dataSource = GetRepository<Repository<OutgoingMessageSuscription>>(factory);
		    var componentSettings = GetComponentSettings();

		    return new CanUpdateOutgoingMessagesSubscriptions(dataSource,factory,componentSettings,GetDomainNotifier(),new ExpressionsHelper());
		}

		protected IDomainObservable GetDomainNotifier()
		{
			//var dataSource = GetRepository<Repository<OutgoingMessageSuscription>>();
			return new DomainNotifier();
		}

		protected IWriteOutgoingQueue GetOutgoingQueueWritter(IUnitOfWorkFactory factory)
		{
			var dataSource = GetRepository<Repository<OutgoingMessage>>(factory);
			var componentSettings = GetComponentSettings();

			return new WriteOutgoingQueue(dataSource, factory, componentSettings);
		}

		protected IReadOutgoingQueue GetOutgoingQueueReader(IUnitOfWorkFactory factory)
		{
			var dataSource = GetRepository<Repository<OutgoingMessage>>(factory);
			var componentSettings = GetComponentSettings();

			return new ReaderOutgoingQueue(dataSource, factory, componentSettings);
		}

		protected ICanReadChunkedMessages  GetChunkedMessagesReader(IUnitOfWorkFactory factory)
		{
			var dataSource = GetRepository<Repository<ChunkedServiceRequestMessageData>>(factory);
			var componentSettings = GetComponentSettings();

			return new ChunkedMessagesReader(dataSource, factory, componentSettings);
		}

		protected ICanWriteChunkedMessages GetChunkedMessagesWritter(IUnitOfWorkFactory factory)
		{
			var dataSource = GetRepository<Repository<ChunkedServiceRequestMessageData>>(factory);
			var componentSettings = GetComponentSettings();

			return new ChunkedMessagesWriter(dataSource, factory, componentSettings);
		}

		protected ICanReadServiceDetails GetServiceDetailsReader(IUnitOfWorkFactory factory)
		{
			var dataSource = GetRepository<Repository<ServiceDetails>>(factory);
			var componentSettings = GetComponentSettings();

			return new ServiceDetailsReader(dataSource, factory, componentSettings);
		}

	}
}