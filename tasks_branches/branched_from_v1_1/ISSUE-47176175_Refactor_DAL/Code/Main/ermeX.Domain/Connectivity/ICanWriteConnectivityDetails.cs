using System;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Connectivity
{
	interface ICanWriteConnectivityDetails
	{
		void RemoveLocalComponentDetails();
		ConnectivityDetails CreateLocalComponentConnectivityDetails(ushort port, bool asLocal=true);
		void RemoveComponentDetails(Guid componentId);
	}
}