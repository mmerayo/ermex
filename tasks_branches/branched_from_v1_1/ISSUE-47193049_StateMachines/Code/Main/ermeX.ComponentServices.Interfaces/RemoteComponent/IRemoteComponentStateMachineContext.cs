using System;
using System.Net;

namespace ermeX.ComponentServices.Interfaces.RemoteComponent
{
	internal interface IRemoteComponentStateMachineContext
	{
		IRemoteComponentStateMachine StateMachine { get; set; }
		Guid ComponentId { get; set; }
		IPAddress IpAddress { get; set; }
		ushort Port { get; set; }
	}
}