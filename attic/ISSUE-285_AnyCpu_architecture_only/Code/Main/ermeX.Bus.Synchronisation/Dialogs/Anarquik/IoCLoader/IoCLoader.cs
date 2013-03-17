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
using ermeX.Bus.Synchronisation.DependencyInjectionModules;
using ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByMessageQueue;
using ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByService;
using ermeX.Bus.Synchronisation.Dialogs.HandledByMessageQueue;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;

namespace ermeX.Bus.Synchronisation.Dialogs.Anarquik.IoCLoader
{
    internal static class IoCLoader
    {
        public static void PerformInjections(DialogInjections injector)
        {
            injector.Bind<IHandshakeService>().To<HandshakeServiceHandler>().InSingletonScope();
            injector.Bind<IMessageSuscriptionsService>().To<MessageSuscriptionsRequestMessageHandler>().InSingletonScope
                ();
            injector.Bind<IPublishedServicesDefinitionsService>().To<PublishedServicesHandler>().InSingletonScope();
            injector.Bind<IUpdatePublishedServiceMessageHandler>().To<UpdatePublishedServiceMessageHandler>().
                InSingletonScope();
            injector.Bind<IUpdateSuscriptionMessageHandler>().To<UpdateSuscriptionMessageHandler>().InSingletonScope();
        }
    }
}