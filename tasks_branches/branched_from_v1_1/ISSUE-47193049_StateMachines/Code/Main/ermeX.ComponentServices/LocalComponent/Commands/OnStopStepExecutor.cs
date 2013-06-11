using System;
using System.Collections.Generic;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers;
using ermeX.Bus.Publishing.Dispatching.Messages;
using ermeX.ComponentServices.Interfaces.LocalComponent.Commands;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.DAL.Interfaces.Component;
using IStartable = ermeX.Common.IStartable;

namespace ermeX.ComponentServices.LocalComponent.Commands
{
	class OnStopStepExecutor : IOnStopStepExecutor
	{
		private IEnumerable<IStartable> _startables;

		[Inject]
		public OnStopStepExecutor(IMessageListener messageListener,
										IMessagePublisher messagePublisher,
										IReceptionMessageDistributor receptionMessageDistributor,
										IMessageSubscribersDispatcher messageSubscribersDispatcher,
										IMessageDistributor messageDistributor)
		{
			
			_startables = new List<IStartable>() //TODO: INJECT AS IStartable[]
				{
					messageListener,
					messagePublisher,
					receptionMessageDistributor,
					messageSubscribersDispatcher,
					messageDistributor
				};
		}

		public void Stop()
		{
			var remoteComponents = ComponentManager.Default.GetRemoteComponents();
			foreach (var remoteComponent in remoteComponents)
				remoteComponent.Stop();

			foreach (var o in _startables)
				o.Stop();
		}
	}
}