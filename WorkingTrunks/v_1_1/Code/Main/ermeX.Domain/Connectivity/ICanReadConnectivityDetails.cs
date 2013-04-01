using System;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Connectivity
{
    interface ICanReadConnectivityDetails
    {
        ConnectivityDetails Fetch(Guid componentId);
    }
}
