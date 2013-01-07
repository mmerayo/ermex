// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Common.Logging;
using ermeX.Common;
using ermeX.Common.Caching;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;


using ermeX.Entities.Entities;
using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Receiving.Server;
using ermeX.Transport.Interfaces.Sending.Client;
using ermeX.Transport.LayerInterfaces;

namespace ermeX.Transport
{
    internal abstract class LoaderBase : IConcreteServiceLoader
    {
        private static volatile List<IServer> _servers;
        private static volatile object _locker = new object();
        protected readonly IConnectivityDetailsDataSource ConnectionsDs;

        protected LoaderBase(ITransportSettings settings, IConnectivityDetailsDataSource connectionsDs,
            IServiceDetailsDataSource servicesDs, ICacheProvider cacheProvider)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (connectionsDs == null) throw new ArgumentNullException("connectionsDs");
            if (servicesDs == null) throw new ArgumentNullException("servicesDs");
            if (cacheProvider == null) throw new ArgumentNullException("cacheProvider");
            ConnectionsDs = connectionsDs;
            Settings = settings;
            ServicesDs = servicesDs;
            CacheProvider = cacheProvider;
        }

        protected ITransportSettings Settings { get; set; }
        protected IServiceDetailsDataSource ServicesDs { get; set; }
        protected ICacheProvider CacheProvider { get; set; }
        protected readonly ILog Logger=LogManager.GetLogger(StaticSettings.LoggerName);

        #region IConcreteServiceLoader Members

        /// <summary>
        ///   It generates a new set of IP:port when loaded first time
        /// </summary>
        /// <returns> </returns>
        public IEnumerable<IServer> LoadServers()
        {
            lock (_locker)
            {
                if (_servers == null)
                    _servers = new List<IServer>();

                var endPoint = ConnectionsDs.GetByComponentId(Settings.ComponentId);
                if (endPoint == null)
                {
                    CreateConnectivitySet();
                    endPoint = ConnectionsDs.GetByComponentId(Settings.ComponentId);
                }
                _servers.Add(GetServer(endPoint));
            }

            return _servers;
        }

        public IEndPoint GetClientProxy(Guid destinationComponent)
        {
            var connectivityDetails = ConnectionsDs.GetByComponentId(destinationComponent);
            var serverInfos = new List<ServerInfo>();


            serverInfos.Add(new ServerInfo
                                {
                                    Ip = connectivityDetails.Ip,
                                    Port = connectivityDetails.Port,
                                    ServerId = connectivityDetails.ServerId
                                });
            return GetClientConcreteProxy(destinationComponent, serverInfos);
        }

        #endregion

        private void CreateConnectivitySet()
        {
            ConnectionsDs.RemoveByProperty("ServerId", Settings.ComponentId.ToString());

            //local
            // CreateConnectivityDetails(IPAddress.Loopback.ToString(),true);

            //IP4
            var localhostIp = Networking.GetLocalhostIp(AddressFamily.InterNetwork);
            CreateConnectivityDetails(localhostIp, false);
        }

        private void CreateConnectivityDetails(string ip, bool isLocal)
        {
            var connectivityDetails = new ConnectivityDetails
                                          {
                                              ServerId = Settings.ComponentId,
                                              ComponentOwner = Settings.ComponentId,
                                              Ip = ip,
                                              IsLocal = isLocal,
                                              Port = Settings.Port
                                          };
            ConnectionsDs.Save(connectivityDetails);
        }


        protected abstract IServer GetServer(ConnectivityDetails item);

        protected abstract int GetPort(ConnectivityDetails item);
        protected abstract IEndPoint GetClientConcreteProxy(Guid serverId, List<ServerInfo> serverInfos);

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                {
                    if (_servers != null)
                        while (_servers.Count > 0)
                        {
                            var server = _servers[0];
                            _servers.RemoveAt(0);
                            server.Dispose();
                        }


                    _disposed = true;
                }
        }

        #endregion
    }
}