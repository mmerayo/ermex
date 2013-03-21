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

using Ninject.Modules;
using ermeX.Domain.Component;
using ermeX.Domain.Connectivity;
using ermeX.Domain.Implementations.Component;
using ermeX.Domain.Implementations.Connectivity;
using ermeX.Domain.Implementations.Messages;
using ermeX.Domain.Implementations.QueryDatabase;
using ermeX.Domain.Implementations.Queues;
using ermeX.Domain.Implementations.Services;
using ermeX.Domain.Implementations.Subscriptions;
using ermeX.Domain.Messages;
using ermeX.Domain.QueryDatabase;
using ermeX.Domain.Queues;
using ermeX.Domain.Services;
using ermeX.Domain.Subscriptions;

namespace ermeX.Domain.IoC
{

	internal class DomainInjections : NinjectModule
	{
		public override void Load()
		{
			//TODO: remove SINGLETON SCOPES
			//COMPONENT
			Bind<ICanReadComponents>().To<ComponentsReader>().InSingletonScope();
			Bind<ICanUpdateComponents>().To<ComponentsUpdater>().InSingletonScope();
			Bind<ICanReadLatency>().To<LatencyReader>().InSingletonScope();
			Bind<ICanUpdateLatency>().To<LatencyUpdater>().InSingletonScope();
			Bind<IRegisterComponents>().To<ComponentsRegistrator>().InSingletonScope();

			//connectivity
			Bind<ICanGetConnectivityDetails>().To<ConnectivityDetailsReader>().InSingletonScope();

			//Messages
			Bind<ICanReadChunkedMessages>().To<ChunkedMessagesReader>().InSingletonScope();

			//queues

			Bind<ICanUpdateOutgoingMessagesQueueInfo>().To<ICanUpdateOutgoingMessagesQueueInfo>().InSingletonScope();
			Bind<IReadIncommingMessagesQueueInfo>().To<ReaderIncommingMessagesQueueInfo>().InSingletonScope();
			Bind<IReadOutgoingMessagesQueueInfo>().To<ReaderOutgoingMessagesQueueInfo>().InSingletonScope();

			//service details
			Bind<ICanReadServiceDetails>().To<CanReadServiceDetails>().InSingletonScope();

			//Subscriptions
			Bind<ICanReadOutgoingMessagesSubscriptions>().To<CanReadOutgoingMessagesSubscriptions>().InSingletonScope();
			Bind<ICanReadIncommingMessagesSubscriptions>().To<CanReadIncommingMessagesSubscriptions>().InSingletonScope();
			Bind<ICanUpdateOutgoingMessagesSubscriptions>().To<CanUpdateOutgoingMessagesSubscriptions>().InSingletonScope();
			Bind<ICanUpdateIncommingMessagesSubscriptions>().To<CanUpdateIncommingMessagesSubscriptions>().InSingletonScope();

			//QueryDatabase

			Bind<IQueryHelperFactory>().To<QueryHelperFactory>().InSingletonScope();
		}
	}
}