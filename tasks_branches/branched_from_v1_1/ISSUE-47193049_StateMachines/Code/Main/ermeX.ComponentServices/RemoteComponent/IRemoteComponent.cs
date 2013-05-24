using System;
using System.Net;

namespace ermeX.ComponentServices.RemoteComponent
{
	internal interface IRemoteComponent : IErmexComponent
	{
		void Join();
		void Create(Guid componentId, IPAddress ipAddress, int port);
	}
}