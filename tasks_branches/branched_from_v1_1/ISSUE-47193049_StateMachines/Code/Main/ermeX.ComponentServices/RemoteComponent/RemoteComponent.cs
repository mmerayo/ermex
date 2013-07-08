using System;
using System.Net;
using System.Threading;

using Ninject;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ConfigurationManagement.Settings;
using ermeX.Logging;
using ermeX.Parallel.Queues;

namespace ermeX.ComponentServices.RemoteComponent
{
	internal class RemoteComponent : IRemoteComponent
	{
		private readonly ILogger Logger;
		private readonly IRemoteComponentStateMachine _stateMachine;

		[Inject]
		public RemoteComponent(IRemoteComponentStateMachine stateMachine,IComponentSettings settings)
		{
			Logger = LogManager.GetLogger<RemoteComponent>(settings.ComponentId, LogComponent.Handshake);

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
						Logger.ErrorFormat("Join -Component:{0}, Exception:{1}",_stateMachine.Context.RemoteComponentId,ex.ToString());
					}
				});
		}

		public void JoinedRemotely()
		{
			try
			{
				_stateMachine.Joined(true);
			}
			catch (Exception ex)
			{
				Logger.ErrorFormat("JoinedRemotely -Component:{0}, Exception:{1}", _stateMachine.Context.RemoteComponentId, ex.ToString());
			}
		}

		public void Create(Guid componentId, IPAddress ipAddress, ushort port)
		{
			_stateMachine.Create(componentId,ipAddress,port);
		}

		public void Stop()
		{
			_stateMachine.Stop();
		}

		
	}
}