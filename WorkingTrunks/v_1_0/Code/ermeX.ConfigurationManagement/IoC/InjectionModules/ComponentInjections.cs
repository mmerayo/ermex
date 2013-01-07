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
using Ninject.Parameters;
using ermeX.Common.Caching;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.ConfigurationManagement.Status;



namespace ermeX.ConfigurationManagement.IoC.InjectionModules
{
    internal class ComponentInjections : NinjectModule
    {
        private readonly IComponentSettings _settings;

        public ComponentInjections(IComponentSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            List<string> errors;
            if (!new ComponentSettingsValidator(settings).Validate(out errors))
            {
                throw new ConfigurationException(string.Join(",", errors));
            }
            _settings = settings;
        }

        public override void Load()
        {
            
            Bind<IComponentSettings>().ToConstant(_settings);

            Bind<IStatusManager>().To<StatusManager>().InSingletonScope();

             
            Bind<ICacheProvider>().ToConstant(new MemoryCacheStore(_settings.CacheExpirationSeconds));


        }
    }
}