using System;

namespace ermeX.Domain.Component
{
    internal interface ICanUpdateLatency
    {
        void RegisterComponentRequestLatency(Guid remoteComponentId, int requestMilliseconds);
    }
}
