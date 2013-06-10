using System;
using ermeX.LayerMessages;

namespace ermeX.ComponentServices.Interfaces.LocalComponent
{
	internal interface ILocalComponent : IErmexComponent
	{
		void Start();
		void PublishMyServices(Guid componentId);
		void PublishMySubscriptions(Guid componentId);
		void Stop();
		bool IsRunning();
		void PublishMessage(BizMessage bizMessage);
		THandler Subscribe<THandler>(Type handlerType);
		object Subscribe(Type handlerType);
		void PublishService<TServiceInterface>(Type serviceImplementationType) where TServiceInterface : IService;
		TService GetServiceProxy<TService>() where TService : IService;
		TService GetServiceProxy<TService>(Guid componentId) where TService : IService;
		bool IsStopped();
	}
}