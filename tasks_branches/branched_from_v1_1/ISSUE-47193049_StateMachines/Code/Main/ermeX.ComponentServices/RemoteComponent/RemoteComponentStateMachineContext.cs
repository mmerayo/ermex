using System;
using System.Net;
using ermeX.ComponentServices.Interfaces.RemoteComponent;

namespace ermeX.ComponentServices.RemoteComponent
{
	/// <summary>
	/// Keeps state machine context info
	/// </summary>
	internal class RemoteComponentStateMachineContext:IRemoteComponentStateMachineContext
	{
		public IRemoteComponentStateMachine StateMachine { get; set; }
		public Guid RemoteComponentId { get; set; }
		public IPAddress RemoteIpAddress { get; set; }
		public ushort RemotePort { get; set; }
		public bool RemoteExecutesJoin { get; set; }
	}
}