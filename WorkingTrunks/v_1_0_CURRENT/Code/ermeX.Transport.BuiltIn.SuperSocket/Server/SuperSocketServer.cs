// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;


using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Reception;

namespace ermeX.Transport.BuiltIn.SuperSocket.Server
{
    internal class SuperSocketServer : ServerBase
    {
        private MainServerHandler _serverHandler;

        public SuperSocketServer(ServerInfo serverInfo, IServiceDetailsDataSource dataSourceServices, IChunkedServiceRequestMessageDataSource chunksDataSource, ITransportSettings settings)
            : base(serverInfo, dataSourceServices,chunksDataSource)
        {
            try
            {
                Settings = settings;
                InitHandler();

                InitRealServer(serverInfo);
            }
            catch (Exception ex)
            {
                Logger.Fatal(x=>x( "cctor.{0}", ex));
                throw ex;
            }
        }

        public SuperSocketServer(ServerInfo serverInfo, IServiceDetailsDataSource dataSourceServices, IChunkedServiceRequestMessageDataSource chunksDataSource)
            : this(serverInfo, dataSourceServices, chunksDataSource,null)
        {
        }

        private ITransportSettings Settings { get; set; }
        private ExposedServer RealServer { get; set; }

        private void InitHandler()
        {
            _serverHandler = new MainServerHandler();
            _serverHandler.RequestReceived += serverHandler_RequestReceived;
            _serverHandler.ChunkReceived += _serverHandler_ChunkReceived;
        }

        private void InitRealServer(ServerInfo serverInfo)
        {
            if (serverInfo == null) throw new ArgumentNullException("serverInfo");

            //TODO: TEST ALL THE MENINGFULL POSSIBLITIES
//TODO: CONFIGURABLE
            var rootConfig = new RootConfig
                                 {
                                     MaxWorkingThreads = 1024,
                                     MaxCompletionPortThreads = 500,
                                     MinWorkingThreads = 15,
                                     MinCompletionPortThreads = 15
                                 };


            var serverConfig = new ServerConfig
                                   {
                                       Ip = "Any",
                                       LogCommand = true,
                                       MaxConnectionNumber = 1024,
                                       Mode = SocketMode.Async,
                                       Name = "Async Socket Server",
                                       Port = serverInfo.Port,
                                       ClearIdleSession = true,
                                       ClearIdleSessionInterval = 300,
                                       //1 minute
                                       IdleSessionTimeOut = 300,
                                       //4 minutes
//#if DEBUG
//                                       MaxCommandLength = Settings!=null? Settings.MaxMessageKbBeforeChunking/1024:8192 //bytes
//#else
                                       MaxCommandLength = Settings.MaxMessageKbBeforeChunking/1024
//#endif
                                   };


            RealServer = new ExposedServer(_serverHandler);
            RealServer.Setup(rootConfig, serverConfig, SocketServerFactory.Instance);
        }

        public override void StartListening()
        {
            try{
            if (RealServer.IsRunning)
                RealServer.Stop();

            RealServer.Start();
            }
            catch (Exception ex)
            {
                Logger.Fatal(x=>x( "StartListening.{0}", ex));
                throw ex;
            }
        }

        public override void Dispose()
        {
            try{
            if (RealServer != null)
            {
                if (RealServer.IsRunning)
                    RealServer.Stop();

                RealServer.Dispose();
                RealServer = null;
            }

            base.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Warn(x=>x("Dispose.{0}", ex));
                throw ex;
            }

        }

        private ServiceResult _serverHandler_ChunkReceived(ChunkedServiceRequestMessage message)
        {
            return base.DoHandleChunk(message);

        }

        private ServiceResult serverHandler_RequestReceived(ServiceRequestMessage message)
        {
            return base.DoHandleRequestTask(message);
        }
    }
}