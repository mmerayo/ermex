using System;
using System.Net;

namespace ermeX.ComponentServices.Interfaces.RemoteComponent
{
	internal interface IRemoteComponentStateMachineContext
	{
		IRemoteComponentStateMachine StateMachine { get; set; }
		Guid RemoteComponentId { get; set; }
		IPAddress RemoteIpAddress { get; set; }
		ushort RemotePort { get; set; }

		/// <summary>
		/// Indicates thet current state machine performs the join
		/// </summary>
		bool RemoteExecutesJoin { get; set; }
	}
}