// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Transport.Interfaces.Sending.Client;
using ermeX.Transport.LayerInterfaces;

namespace ermeX.Transport.Publish
{
    internal class ProxyProvider : IProxyProvider
    {
        [Inject]
        public ProxyProvider(ITransportSettings settings,
                             IConnectivityDetailsDataSource connectivityDetailsDataSource,
                             IEnumerable<IConcreteServiceLoader> serviceLoaders)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (connectivityDetailsDataSource == null) throw new ArgumentNullException("connectivityDetailsDataSource");
            if (serviceLoaders == null) throw new ArgumentNullException("serviceLoaders");
            Settings = settings;
            DataSource = connectivityDetailsDataSource;
            ServiceLoaders = serviceLoaders;
        }

        private IConnectivityDetailsDataSource DataSource { get; set; }
        private IEnumerable<IConcreteServiceLoader> ServiceLoaders { get; set; }

        private ITransportSettings Settings { get; set; }

        #region IProxyProvider Members

        public List<IEndPoint> GetClientProxies(Guid destinationComponent)
        {
            var result = new List<IEndPoint>();
            foreach (var concreteServiceLoader in ServiceLoaders)
            {
                result.Add(concreteServiceLoader.GetClientProxy(destinationComponent));
            }

            return result;
        }

        #endregion
    }
}