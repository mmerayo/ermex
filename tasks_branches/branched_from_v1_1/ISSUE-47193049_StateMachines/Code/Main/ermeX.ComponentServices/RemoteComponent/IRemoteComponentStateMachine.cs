using System;

namespace ermeX.ComponentServices.RemoteComponent
{
	internal interface IRemoteComponentStateMachine
	{
		void Create();
		void Join();
		void Stop();

		bool IsErrored();
		bool IsStopped();
		bool IsRunning();
	}
}