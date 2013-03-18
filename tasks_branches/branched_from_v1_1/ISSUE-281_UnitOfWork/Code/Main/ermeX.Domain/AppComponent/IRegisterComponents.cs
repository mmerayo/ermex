using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Domain.AppComponent
{
    internal interface IRegisterComponents //TODO: sEE AUTOREGISTRATION
    {
        bool CreateRemoteComponent(Guid remoteComponentId, string ip, int port);
       
        void CreateLocalComponent(int port);
    }
}
