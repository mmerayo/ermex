using System;
using Ninject;
using ermeX.DAL.Interfaces.Component;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	internal class OnCreatingStepExecutor : IOnCreatingStepExecutor
	{
		private readonly IRegisterComponents _componentRegistrator;

		[Inject]
		public OnCreatingStepExecutor(IRegisterComponents componentRegistrator)
		{
			_componentRegistrator = componentRegistrator;
		}

		public void Create(IRemoteComponentStateMachineContext context)
		{
			_componentRegistrator.CreateRemoteComponent(context.ComponentId,
			                                            context.IpAddress.ToString(),
			                                            context.Port);
		}
	}
}