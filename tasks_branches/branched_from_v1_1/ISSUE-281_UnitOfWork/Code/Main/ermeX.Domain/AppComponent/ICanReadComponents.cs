using System;
using System.Collections.Generic;

namespace ermeX.Domain.AppComponent
{
    internal interface ICanReadComponents
    {
        Entities.Entities.AppComponent Fetch(Guid componentId);
        List<Entities.Entities.AppComponent> FetchOtherComponents();
        List<Entities.Entities.AppComponent> FetchOtherComponentsNotExchangedDefinitions(bool running = false);

    }
}
