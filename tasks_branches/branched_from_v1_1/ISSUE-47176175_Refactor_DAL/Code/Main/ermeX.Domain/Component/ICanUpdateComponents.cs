using System;
using ermeX.ConfigurationManagement.Status;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Component
{
    //TODO: SIMPLIFY
    internal interface ICanUpdateComponents
    {
        bool ImportFromOtherComponent(Entities.Entities.AppComponent entity, Tuple<string, object>[] deterministicFilter, ConnectivityDetails connectivityDetails);
        void SetComponentRunningStatus(Guid componentId, ComponentStatus newStatus, bool exchangedDefinitions = false);
	    void Save(AppComponent component);
    }
}
