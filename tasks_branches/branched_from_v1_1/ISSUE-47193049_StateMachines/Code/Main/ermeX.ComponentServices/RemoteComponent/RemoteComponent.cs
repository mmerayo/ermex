using System;
using System.Net;
using System.Threading;
using Ninject;

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
			var joinThread = new Thread(() => _stateMachine.Join());
			joinThread.Start();
		}

		public void Create(Guid componentId, IPAddress ipAddress, ushort port)
		{
			_stateMachine.Create(componentId,ipAddress,port);
		}
	}
}