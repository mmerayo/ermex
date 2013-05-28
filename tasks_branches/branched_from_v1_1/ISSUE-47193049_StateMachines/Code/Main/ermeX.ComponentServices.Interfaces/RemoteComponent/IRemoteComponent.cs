using System;
using System.Net;

namespace ermeX.ComponentServices.Interfaces.RemoteComponent
{
	internal interface IRemoteComponent : IErmexComponent
	{
		void Join();
		void Create(Guid componentId, IPAddress ipAddress, ushort port);
	}
}