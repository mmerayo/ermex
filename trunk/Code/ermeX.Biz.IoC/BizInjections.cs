// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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
            Bind<IServicePublishingManager>().To<ServicesPublishing.Manager>().InSingletonScope();
            Bind<IServicesManager>().To<Services.Manager>().InSingletonScope();
            Bind<ISubscriptionsManager>().To<Subscriptions.Manager>().InSingletonScope();
            Bind<IComponentManager>().To<ComponentManager>().InSingletonScope();//(TypesHelper.GetTypeFromDomainByClassName("ComponentStarter")).InSingletonScope();
        }
    }
}