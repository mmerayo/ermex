namespace ermeX.ComponentServices.LocalComponent
{
	internal interface ILocalComponentStateMachine
	{
		bool IsErrored();
		void Start();
		bool IsStopped();
		bool IsRunning();
		void Stop();
	}
}