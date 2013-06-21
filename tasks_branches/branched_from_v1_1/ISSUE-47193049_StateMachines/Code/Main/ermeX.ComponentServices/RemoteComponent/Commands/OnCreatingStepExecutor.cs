using System;
using Ninject;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;
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
			_componentRegistrator.CreateRemoteComponent(context.RemoteComponentId,
			                                            context.RemoteIpAddress.ToString(),
			                                            context.RemotePort);
		}
	}
}