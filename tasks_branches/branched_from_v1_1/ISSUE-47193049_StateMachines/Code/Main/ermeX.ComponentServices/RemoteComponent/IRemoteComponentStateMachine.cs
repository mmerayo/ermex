﻿using System;

namespace ermeX.ComponentServices.RemoteComponent
{
	internal interface IRemoteComponentStateMachine
	{
		void Create();
		void Join();
		void Joined();
		void Stop();

		bool IsErrored();
		bool IsStopped();
		bool IsRunning();
		bool WasCreated();
		bool IsJoining();
		bool IsRequestingServices();
		void SubscriptionsReceived();
		void ServicesReceived();
	}
}