using System;

namespace ermeX.DAL.Interfaces.Component
{
    internal interface ICanUpdateLatency
    {
        void RegisterComponentRequestLatency(Guid remoteComponentId, int requestMilliseconds);
    }
}
