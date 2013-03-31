using System;
using System.Collections.Generic;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Component
{
	internal interface ICanReadComponents
	{
		AppComponent Fetch(Guid componentId);
		IList<AppComponent> FetchAll();
		IList<AppComponent> FetchOtherComponents();
		IList<AppComponent> FetchOtherComponentsNotExchangedDefinitions(bool running = false);
	}
}