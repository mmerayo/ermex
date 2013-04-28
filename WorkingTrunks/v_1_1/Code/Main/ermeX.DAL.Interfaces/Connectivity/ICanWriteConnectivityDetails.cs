using System;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces.Connectivity
{
	interface ICanWriteConnectivityDetails
	{
		void RemoveLocalComponentDetails();
		ConnectivityDetails CreateComponentConnectivityDetails(ushort port, bool asLocal = true);
		void RemoveComponentDetails(Guid componentId);
	}
}