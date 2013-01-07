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
using ermeX.Transport.BuiltIn.SuperSocket;
using ermeX.Transport.Interfaces;
using ermeX.Transport.Interfaces.Sending.Client;
using ermeX.Transport.LayerInterfaces;
using ermeX.Transport.Publish;
using ermeX.Transport.Reception;

namespace ermeX.Transport.IoC
{
    internal class TransportInjections : NinjectModule
    {
        private ITransportSettings _settings;

        public TransportInjections(ITransportSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            List<string> errors;
            if (!new TransportSettingsValidator(settings).Validate(out errors))
            {
                throw new ConfigurationException(string.Join(",", errors));
            }
            _settings = settings;
        }
        public override void Load()
        {
            Bind<IConcreteServiceLoader>().To<SocketsConcreteLoader>().InSingletonScope();
            Bind<IProxyProvider>().To<ProxyProvider>().InSingletonScope();
            Bind<IServiceProxy>().To<ServiceProxy>();
            Bind<IConnectivityManager>().To<ConnectivityManager>();
            Bind<ITransportSettings>().ToConstant(_settings);
        }
    }
}