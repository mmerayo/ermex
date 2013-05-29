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
using ermeX.Biz.Interfaces;
using ermeX.Biz.Messaging;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;

namespace ermeX.Biz.IoC
{
    internal class BizInjections : NinjectModule
    {
         private readonly IBizSettings _settings;

         public BizInjections(IBizSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            List<string> errors;
            if (!new BizSettingsValidator(settings).Validate(out errors))
            {
                throw new ConfigurationException(string.Join(",", errors));
            }
            _settings = settings;
        }

        public override void Load()
        {
            Bind<IMessagingManager>().To<Manager>().InSingletonScope();
            Bind<IServicePublishingManager>().To<ServicesPublishing.ServicePublishingManager>().InSingletonScope();
            Bind<IServicesManager>().To<Services.Manager>().InSingletonScope();
            Bind<ISubscriptionsManager>().To<Subscriptions.SubscriptionsManager>().InSingletonScope();
        }
    }
}