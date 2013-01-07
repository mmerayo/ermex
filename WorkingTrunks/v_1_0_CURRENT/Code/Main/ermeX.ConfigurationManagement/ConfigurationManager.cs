// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Ninject.Modules;
using ermeX.Common;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;

namespace ermeX.ConfigurationManagement
{
    internal class ConfigurationManager : IConfigurationManager
    {
        private static IConfigurationManager _instance;

        private IComponentSettings _settings;

        private ConfigurationManager(IComponentSettings settings)
        {
            _settings = settings;
        }

        public static IConfigurationManager Instance
        {
            get { return _instance; }
        }

        #region IConfigurationManager Members

        public void ClearConfiguration()
        {
            //TODO:
        }

        #endregion

        /// <summary>
        ///   It performs and updates the whole Service configuration
        /// </summary>
        /// <param name="settings"> </param>
        public static void SetSettingsSource(IComponentSettings settings)
        {
            //TODO  locker here
            if (settings == null) throw new ArgumentNullException("settings");
            List<string> errors;
            if (!new ComponentSettingsValidator(settings).Validate(out errors))
            {
                throw new ConfigurationException(string.Join(",", errors));
            }

            if (settings.ConfigurationManagerType == null)
                _instance = new ConfigurationManager(settings);
            else
            {
                _instance = ObjectBuilder.FromType<IConfigurationManager>(settings.ConfigurationManagerType);
            }


            
            INinjectModule[] injectionModules = GetInjectionModules(settings as IBizSettings);
            IoCManager.SetCurrentInjections(injectionModules);


            //Already disposed from previous execution and reinjected CacheProvider.ClearCache();
        }

        private static INinjectModule[] GetInjectionModules(IBizSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            Type[] moduleTypes =
                TypesHelper.GetTypesFromDomainImplementing<INinjectModule>().Where(
                    x => !x.Namespace.StartsWith("Ninject")).ToArray();

            var result = new List<INinjectModule>(moduleTypes.Length);
            result.AddRange(
                moduleTypes.Select(moduleType => ObjectBuilder.FromType<INinjectModule>(moduleType, settings)));

            return result.ToArray();
        }
    }
}