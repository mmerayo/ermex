using System;
using System.Net;

namespace ermeX.ComponentServices.RemoteComponent
{
	/// <summary>
	/// Keeps state machine context info
	/// </summary>
	internal class RemoteComponentStateMachineContext:IRemoteComponentStateMachineContext
	{
		public IRemoteComponentStateMachine StateMachine { get; set; }
		public Guid ComponentId { get; set; }
		public IPAddress IpAddress { get; set; }
		public ushort Port { get; set; }
	}
}