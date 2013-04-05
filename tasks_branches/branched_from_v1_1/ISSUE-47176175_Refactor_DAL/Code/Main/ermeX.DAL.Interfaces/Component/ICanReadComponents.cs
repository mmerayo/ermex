using System;
using System.Collections.Generic;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Component
{
	internal interface ICanReadComponents
	{
		AppComponent Fetch(Guid componentId);
		IEnumerable<AppComponent> FetchAll();
		IEnumerable<AppComponent> FetchOtherComponents();
		IEnumerable<AppComponent> FetchOtherComponentsNotExchangedDefinitions(bool running = false);
	}
}