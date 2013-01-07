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
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data;

namespace ermeX.ConfigurationManagement.IoC.InjectionModules
{
    internal class ConfigurationSchemaInjections : NinjectModule
    {
        private readonly IDalSettings _settings;

        public ConfigurationSchemaInjections(IDalSettings settings) //TODO: TO DAL IOC
        {
            if (settings == null) throw new ArgumentNullException("settings");
            List<string> errors;
            if (!new DataAccessSettingsValidator(settings).Validate(out errors))
            {
                throw new ConfigurationException(string.Join(",", errors));
            }
            _settings = settings;
        }

        public override void Load()
        {
            //TODO: CHANGE THIS when configuration has values passing the whole section
            Bind<IDalSettings>().ToConstant(_settings);
        }
    }
}