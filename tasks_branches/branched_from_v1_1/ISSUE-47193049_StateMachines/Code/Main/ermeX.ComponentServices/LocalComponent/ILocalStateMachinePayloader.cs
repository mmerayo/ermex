﻿namespace ermeX.ComponentServices.LocalComponent
{
	internal interface ILocalStateMachinePayloader
	{
		void Reset();
		void Stop();
		void Run();
		void PublishServices();
		void SubscribeToMessages();
		void OnStart();
		void Error();
	}
}