using System;
using ermeX.ConfigurationManagement.Status;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Component
{
    internal interface ICanWriteComponents
    {
        bool ImportFromOtherComponent(AppComponent entity, ConnectivityDetails connectivityDetails);
        void SetComponentRunningStatus(Guid componentId, ComponentStatus newStatus, bool exchangedDefinitions = false);
	    void Save(AppComponent component);
    }
}
