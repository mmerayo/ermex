// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Entities.Entities;

namespace ermeX.Transport.Interfaces.Entities
{
    internal class ServerInfo
    {
        public ServerInfo()
        {
        }

        public ServerInfo(ConnectivityDetails details)
        {
            if (details == null) throw new ArgumentNullException("details");
            ServerId = details.ServerId;
            Ip = details.Ip;
            Port = details.Port;
            IsLocal = details.IsLocal;
        }

        public Guid ServerId { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public bool IsLocal { get; set; } //TODO:CHANGE THIS TO PROTOCOL 
    }
}