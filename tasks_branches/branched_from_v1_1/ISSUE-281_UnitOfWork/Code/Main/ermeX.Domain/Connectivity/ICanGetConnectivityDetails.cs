using System;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Connectivity
{
    interface ICanGetConnectivityDetails
    {
        ConnectivityDetails Fetch(Guid componentId);
    }
}
