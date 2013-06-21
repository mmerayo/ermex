using System;
using System.Net;

namespace ermeX.ComponentServices.Interfaces.RemoteComponent
{
	internal interface IRemoteComponentStateMachine
	{
		void Create(Guid componentId, IPAddress ipAddress, ushort port);
		void Join();
		/// <summary>
		/// 
		/// </summary>
		/// <param name="remotely">The remote component carried out the handshake</param>
		void Joined(bool remotely);
		void Stop();

		bool IsErrored();
		bool IsStopped();
		bool IsRunning();
		bool WasCreated();
		bool IsJoining();
		bool IsRequestingServices();
		bool IsRequestingSubscriptions();

		void SubscriptionsReceived();
		void ServicesReceived();
		IRemoteComponentStateMachineContext Context { get; }
	}
}