using System;

namespace ermeX.ComponentServices.Interfaces.LocalComponent
{
	internal interface ILocalComponent : IErmexComponent
	{
		void Start();
		void PublishMyServices(Guid componentId);
		void PublishMySubscriptions(Guid componentId);
		void Stop();
		bool IsRunning();
	}
}