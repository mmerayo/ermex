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
using ermeX.DAL.DataAccess.DataSources;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.DAL.DataAccess.Providers;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces;
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

			//Bind<IAppComponentDataSource>().To<AppComponentDataSource>().InSingletonScope();
			//Bind<IConnectivityDetailsDataSource>().To<ConnectivityDetailsDataSource>().InSingletonScope();
			//Bind<IOutgoingMessageSuscriptionsDataSource>().To<OutgoingMessageSuscriptionsDataSource>().InSingletonScope();
			//Bind<IIncomingMessageSuscriptionsDataSource>().To<IncomingMessageSuscriptionsDataSource>().InSingletonScope();
			//Bind<IOutgoingMessagesDataSource>().To<OutgoingMessagesDataSource>().InSingletonScope();
			//Bind<IIncomingMessagesDataSource>().To<IncomingMessagesDataSource>().InSingletonScope();
			//Bind<IServiceDetailsDataSource>().To<ServiceDetailsDataSource>().InSingletonScope();
			//Bind<IChunkedServiceRequestMessageDataSource>().To<ChunkedServiceRequestMessageDataSource>().InSingletonScope();
            
			//Bind<IAutoRegistration>().To<AutoRegistration>().InSingletonScope();
			//Bind<IDataAccessExecutor>().To<DataAccessExecutor>().InSingletonScope();

	        BindRepository<AppComponent>();
			BindRepository<ConnectivityDetails>();
			BindRepository<OutgoingMessageSuscription>();
			BindRepository<IncomingMessageSuscription>();
			BindRepository<OutgoingMessage>();
			BindRepository<IncomingMessage>();
			BindRepository<ServiceDetails>();
			BindRepository<ChunkedServiceRequestMessageData>();

			Bind<ISessionProvider>().To<SessionProvider>();
	        Bind<IUnitOfWorkFactory>().To<UnitOfWorkFactory>();
	        
        }

		private void BindRepository<TModel>() where TModel : ModelBase
		{
			Bind<IReadOnlyRepository<TModel>>().To<Repository<TModel>>().InSingletonScope();
			Bind<IPersistRepository<TModel>>().To<Repository<TModel>>().InSingletonScope();
		}
    }
}