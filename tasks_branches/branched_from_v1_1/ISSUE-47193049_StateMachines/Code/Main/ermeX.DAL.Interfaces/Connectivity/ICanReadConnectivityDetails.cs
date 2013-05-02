using System;
using ermeX.Models.Entities;

namespace ermeX.DAL.Interfaces.Connectivity
{
    interface ICanReadConnectivityDetails
    {
        ConnectivityDetails Fetch(Guid componentId);
    }
}
