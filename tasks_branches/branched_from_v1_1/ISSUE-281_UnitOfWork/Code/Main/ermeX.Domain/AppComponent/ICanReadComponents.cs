using System;
using System.Collections.Generic;

namespace ermeX.Domain.AppComponent
{
    internal interface ICanReadComponents
    {
        Entities.Entities.AppComponent Fetch(Guid componentId);
        IList<Entities.Entities.AppComponent> FetchOtherComponents();
        IList<Entities.Entities.AppComponent> FetchOtherComponentsNotExchangedDefinitions(bool running = false);

    }
}
