using System;
using ermeX.ConfigurationManagement.Status;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces.Component
{
    internal interface ICanWriteComponents
    {
        void SetComponentRunningStatus(Guid componentId, ComponentStatus newStatus, bool exchangedDefinitions = false);
	    void Save(AppComponent component);
    }
}
