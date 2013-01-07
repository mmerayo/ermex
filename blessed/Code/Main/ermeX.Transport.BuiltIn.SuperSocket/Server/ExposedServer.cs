// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using ermeX.Transport.Interfaces.Receiving.Server;
using ermeX.Transport.Reception;

namespace ermeX.Transport.BuiltIn.SuperSocket.Server
{
    internal class ExposedServer : AppServer<ExposedSession, BinaryCommandInfo>, IServerHandlerProvider
    {
        private readonly MainServerHandler _serverHandler;

        public ExposedServer(MainServerHandler serverHandler) : base(new DataReceptionProtocol())
        {
            if (serverHandler == null) throw new ArgumentNullException("serverHandler");
            _serverHandler = serverHandler;
        }

        #region IServerHandlerProvider Members

        public IServerHandlerContract ServerHandler
        {
            get { return _serverHandler; }
        }

        #endregion
    }
}