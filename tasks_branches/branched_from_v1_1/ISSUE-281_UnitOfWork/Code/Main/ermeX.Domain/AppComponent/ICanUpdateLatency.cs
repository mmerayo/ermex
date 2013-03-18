using System;

namespace ermeX.Domain.AppComponent
{
    internal interface ICanUpdateLatency
    {
        void RegisterComponentRequestLatency(Guid componentId, int requestMilliseconds);
    }
}
