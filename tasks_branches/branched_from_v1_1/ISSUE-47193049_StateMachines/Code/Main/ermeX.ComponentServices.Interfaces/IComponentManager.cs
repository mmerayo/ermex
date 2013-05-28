using System;
using System.Net;
using ermeX.ComponentServices.Interfaces.LocalComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.Configuration;

namespace ermeX.ComponentServices.Interfaces
{
	internal interface IComponentManager
	{
		void Setup(Configurer settings);
		void Reset();
		bool IsRunning();
		ILocalComponent LocalComponent { get; }
		IRemoteComponent FriendComponent { get; }
		IRemoteComponent GetRemoteComponent(Guid componentId);
		bool AddRemoteComponent(Guid componentId, IPAddress address, ushort port,bool joinIfCreated=false);
	}
}