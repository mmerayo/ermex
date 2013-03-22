using System;

namespace ermeX.Domain.Component
{
    internal interface IRegisterComponents //TODO: sEE AUTOREGISTRATION
    {
        bool CreateRemoteComponent(Guid remoteComponentId, string ip, int port);
       
        void CreateLocalComponent(int port);


    }
}
