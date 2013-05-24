using System;
using System.Net;

namespace ermeX.ComponentServices.RemoteComponent
{
	internal interface IRemoteComponentStateMachine
	{
		void Create(Guid componentId, IPAddress ipAddress, ushort port);
		void Join();
		void Joined();
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
	}
}