using System;
using System.Net;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.Parallel.Queues;

namespace ermeX.ComponentServices.RemoteComponent
{
	internal class RemoteComponent : IRemoteComponent
	{
		private static readonly ILog Logger = LogManager.GetLogger<RemoteComponent>();
		private readonly IRemoteComponentStateMachine _stateMachine;

		[Inject]
		public RemoteComponent(IRemoteComponentStateMachine stateMachine)
		{
			_stateMachine = stateMachine;
		}

		public void Join()
		{
			SystemTaskQueue.Instance.EnqueueItem(() =>
				{
					try
					{
						_stateMachine.Join();
					}
					catch (Exception ex)
					{
						Logger.ErrorFormat("Join -Component:{0}, Exception:{1}",_stateMachine.Context.ComponentId,ex.ToString());
					}
				});
		}

		public void Create(Guid componentId, IPAddress ipAddress, ushort port)
		{
			_stateMachine.Create(componentId,ipAddress,port);
		}
	}
}