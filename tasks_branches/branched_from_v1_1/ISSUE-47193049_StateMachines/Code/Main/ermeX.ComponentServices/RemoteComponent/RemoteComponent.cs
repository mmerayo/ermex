using System;
using System.Net;
using Ninject;
using ermeX.DAL.Interfaces.Component;

namespace ermeX.ComponentServices.RemoteComponent
{
	internal class RemoteComponent : IRemoteComponent
	{
		private readonly IRemoteComponentStateMachine _stateMachine;

		[Inject]
		public RemoteComponent(IRemoteComponentStateMachine stateMachine)
		{
			_stateMachine = stateMachine;
		}

		public void Join()
		{
			throw new System.NotImplementedException();
		}

		public void Create(Guid componentId, IPAddress ipAddress, int port)
		{
			_stateMachine.Create(componentId,ipAddress,port);
		}
	}
}