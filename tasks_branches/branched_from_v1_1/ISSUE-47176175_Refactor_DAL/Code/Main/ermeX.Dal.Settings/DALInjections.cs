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
using System.Configuration;
using Ninject.Modules;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.Commands.Component;
using ermeX.DAL.Commands.Connectivity;
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
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Interfaces.Connectivity;
using ermeX.DAL.Interfaces.Messages;
using ermeX.DAL.Interfaces.Observers;
using ermeX.DAL.Interfaces.QueryDatabase;
using ermeX.DAL.Interfaces.Queues;
using ermeX.DAL.Interfaces.Services;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.Entities.Base;
using ermeX.Entities.Entities;

namespace ermeX.Dal.Settings
{
	//TODO: TO BE MOVED TO ANOTHER ASSEMBLY
    internal class DALInjections : NinjectModule
    {
         private readonly IDalSettings _settings;

         public DALInjections(IDalSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            List<string> errors;
            if (!new DalSettingsValidator(settings).Validate(out errors))
            {
                throw new ConfigurationException(string.Join(",", errors));
            }
            _settings = settings;
        }

        public override void Load()
        {
            //TODO: create interfaces and bind

			
	        BindRepository<AppComponent>();
			BindRepository<ConnectivityDetails>();
			BindRepository<OutgoingMessageSuscription>();
			BindRepository<IncomingMessageSuscription>();
			BindRepository<OutgoingMessage>();
			BindRepository<IncomingMessage>();
			BindRepository<ServiceDetails>();
			BindRepository<ChunkedServiceRequestMessageData>();

	        Bind<ISessionProvider>().To<SessionProvider>().InSingletonScope();
	        Bind<IUnitOfWorkFactory>().To<UnitOfWorkFactory>().InSingletonScope();

			Bind<ICanReadComponents>().To<ComponentsReader>();
			Bind<ICanWriteComponents>().To<ComponentsUpdater>();
			Bind<ICanReadLatency>().To<LatencyReader>();
			Bind<ICanUpdateLatency>().To<LatencyUpdater>();
			Bind<IRegisterComponents>().To<ComponentsRegistrator>();

			//connectivity
			Bind<ICanReadConnectivityDetails>().To<ConnectivityDetailsReader>();
			Bind<ICanWriteConnectivityDetails>().To<ConnectivityDetailsWritter>();

			//Messages
			Bind<ICanReadChunkedMessages>().To<ChunkedMessagesReader>();
			Bind<ICanWriteChunkedMessages>().To<ChunkedMessagesWriter>();

			//queues
			Bind<IReadOutgoingQueue>().To<ReaderOutgoingQueue>();
			Bind<IWriteOutgoingQueue>().To<WriteOutgoingQueue>();

			Bind<IReadIncommingQueue>().To<ReaderIncommingQueue>();
			Bind<IWriteIncommingQueue>().To<IncommingQueueWriter>();

			//service details
			Bind<ICanReadServiceDetails>().To<ServiceDetailsReader>();
			Bind<ICanWriteServiceDetails>().To<ServiceDetailsWriter>();

			//Subscriptions
			Bind<ICanReadOutgoingMessagesSubscriptions>().To<CanReadOutgoingMessagesSubscriptions>();
			Bind<ICanReadIncommingMessagesSubscriptions>().To<CanReadIncommingMessagesSubscriptions>();
			Bind<ICanUpdateOutgoingMessagesSubscriptions>().To<CanUpdateOutgoingMessagesSubscriptions>();
			Bind<ICanUpdateIncommingMessagesSubscriptions>().To<CanUpdateIncommingMessagesSubscriptions>();

			//QueryDatabase
			Bind<IQueryHelperFactory>().To<QueryHelperFactory>().InSingletonScope();

			//ExpressionsHelper
	        var expressionsHelper = new ExpressionsHelper();

	        Bind<IExpressionHelper<AppComponent>>().ToConstant(expressionsHelper);
			Bind<IExpressionHelper<ChunkedServiceRequestMessageData>>().ToConstant(expressionsHelper);
			Bind<IExpressionHelper<ConnectivityDetails>>().ToConstant(expressionsHelper);
			Bind<IExpressionHelper<IncomingMessage>>().ToConstant(expressionsHelper);
			Bind<IExpressionHelper<IncomingMessageSuscription>>().ToConstant(expressionsHelper);
			Bind<IExpressionHelper<OutgoingMessage>>().ToConstant(expressionsHelper);
			Bind<IExpressionHelper<OutgoingMessageSuscription>>().ToConstant(expressionsHelper);
			Bind<IExpressionHelper<ServiceDetails>>().ToConstant(expressionsHelper);

			//Notifiers
			//TODO: REMOVE
			Bind<IDomainObservable>().To<DomainNotifier>().InSingletonScope();

	        BindDependentOnDbServices();

        }

	    private void BindDependentOnDbServices()
	    {
		    switch(_settings.ConfigurationSourceType)
		    {
			    case DbEngineType.SqlServer2008:
				    Bind<IWriteTransactionProvider>().To<GenericTransactionProvider>().InSingletonScope();
					Bind<IReadTransactionProvider>().To<GenericTransactionProvider>().InSingletonScope();
				    break;
			    case DbEngineType.Sqlite:
			    case DbEngineType.SqliteInMemory:

					//TODO: THIS IS THE VALID ONE? Bind<IWriteTransactionProvider>().To<MutexedTransactionProvider>().InSingletonScope();
					Bind<IReadTransactionProvider>().To<GenericTransactionProvider>().InSingletonScope();
					Bind<IWriteTransactionProvider>().To<MutexedTransactionProvider>().InSingletonScope();
				    break;
			    default:
				    throw new ArgumentOutOfRangeException();
		    }
	    }

	    private void BindRepository<TModel>() where TModel : ModelBase
		{
			Bind<IReadOnlyRepository<TModel>>().To<Repository<TModel>>().InSingletonScope();
			Bind<IPersistRepository<TModel>>().To<Repository<TModel>>().InSingletonScope();
		}
    }
}