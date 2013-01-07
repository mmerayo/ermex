// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ermeX.ConfigurationManagement.Status;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces
{
    internal interface IAppComponentDataSource : IDataSource<AppComponent>
    {
        AppComponent GetByComponentId(Guid componentId);
        int GetMaxLatency();
        IList<AppComponent> GetOthers();

        bool SaveFromOtherComponent(AppComponent entity, Tuple<string, object>[] deterministicFilter,ConnectivityDetails connectivityDetails);
        IEnumerable<AppComponent> GetOtherComponentsWhereDefinitionsNotExchanged(bool running=false);
        void SetComponentRunningStatus(Guid componentId, ComponentStatus newStatus, bool exchangedDefinitions = false);

        void UpdateRemoteComponentLatency(Guid remoteComponentId, int currentRequestMilliseconds);
    }
}