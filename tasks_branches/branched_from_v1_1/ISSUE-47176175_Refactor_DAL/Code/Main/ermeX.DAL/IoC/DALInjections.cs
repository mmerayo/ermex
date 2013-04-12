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
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Commands.Component;
using ermeX.DAL.Commands.Connectivity;
using ermeX.DAL.Commands.Messages;
using ermeX.DAL.Commands.Observers;
using ermeX.DAL.Commands.QueryDatabase;
using ermeX.DAL.Commands.Queues;
using ermeX.DAL.Commands.Services;
using ermeX.DAL.Commands.Subscriptions;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.DAL.DataAccess.Providers;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
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

namespace ermeX.DAL.IoC
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
	        Bind<IUnitOfWorkFactory>().To<UnitOfWorkFactory>();

			Bind<ICanReadComponents>().To<ComponentsReader>().InSingletonScope();
			Bind<ICanWriteComponents>().To<ComponentsUpdater>().InSingletonScope();
			Bind<ICanReadLatency>().To<LatencyReader>().InSingletonScope();
			Bind<ICanUpdateLatency>().To<LatencyUpdater>().InSingletonScope();
			Bind<IRegisterComponents>().To<ComponentsRegistrator>().InSingletonScope();

			//connectivity
			Bind<ICanReadConnectivityDetails>().To<ConnectivityDetailsReader>().InSingletonScope();
			Bind<ICanWriteConnectivityDetails>().To<ConnectivityDetailsWritter>().InSingletonScope();

			//Messages
			Bind<ICanReadChunkedMessages>().To<ChunkedMessagesReader>().InSingletonScope();
			Bind<ICanWriteChunkedMessages>().To<ChunkedMessagesWriter>().InSingletonScope();

			//queues

			Bind<IReadOutgoingQueue>().To<ReaderOutgoingQueue>().InSingletonScope();
			Bind<IWriteOutgoingQueue>().To<WriteOutgoingQueue>().InSingletonScope();

			Bind<IReadIncommingQueue>().To<ReaderIncommingQueue>().InSingletonScope();
			Bind<IWriteIncommingQueue>().To<IncommingQueueWriter>().InSingletonScope();

			//service details
			Bind<ICanReadServiceDetails>().To<ServiceDetailsReader>().InSingletonScope();
			Bind<ICanWriteServiceDetails>().To<ServiceDetailsWriter>().InSingletonScope();

			//Subscriptions
			Bind<ICanReadOutgoingMessagesSubscriptions>().To<CanReadOutgoingMessagesSubscriptions>().InSingletonScope();
			Bind<ICanReadIncommingMessagesSubscriptions>().To<CanReadIncommingMessagesSubscriptions>().InSingletonScope();
			Bind<ICanUpdateOutgoingMessagesSubscriptions>().To<CanUpdateOutgoingMessagesSubscriptions>().InSingletonScope();
			Bind<ICanUpdateIncommingMessagesSubscriptions>().To<CanUpdateIncommingMessagesSubscriptions>().InSingletonScope();

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
	        
        }

		private void BindRepository<TModel>() where TModel : ModelBase
		{
			Bind<IReadOnlyRepository<TModel>>().To<Repository<TModel>>().InSingletonScope();
			Bind<IPersistRepository<TModel>>().To<Repository<TModel>>().InSingletonScope();
		}
    }
}