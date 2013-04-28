using System;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces.Connectivity
{
    interface ICanReadConnectivityDetails
    {
        ConnectivityDetails Fetch(Guid componentId);
    }
}
