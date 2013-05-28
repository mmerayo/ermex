using System;
using System.Collections.Generic;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers;
using ermeX.Bus.Publishing.Dispatching.Messages;
using ermeX.ComponentServices.Interfaces.LocalComponent.Commands;
using ermeX.DAL.Interfaces.Component;
using IStartable = ermeX.Common.IStartable;

namespace ermeX.ComponentServices.LocalComponent.Commands
{
	class OnStopStepExecutor : IOnStopStepExecutor
	{
		private readonly IRegisterComponents _componentsRegister;
		private readonly IMessageListener _messageListener;
		private readonly IMessagePublisher _messagePublisher;
		private readonly IReceptionMessageDistributor _receptionMessageDistributor;
		private readonly IMessageSubscribersDispatcher _messageSubscribersDispatcher;
		private readonly IMessageDistributor _messageDistributor;
		private IEnumerable<IStartable> _startables;


		[Inject]
		public OnStopStepExecutor(IRegisterComponents componentsRegister,
										IMessageListener messageListener,
										IMessagePublisher messagePublisher,
										IReceptionMessageDistributor receptionMessageDistributor,
										IMessageSubscribersDispatcher messageSubscribersDispatcher,
										IMessageDistributor messageDistributor)
		{
			_componentsRegister = componentsRegister;
			_messageListener = messageListener;
			_messagePublisher = messagePublisher;
			_receptionMessageDistributor = receptionMessageDistributor;
			_messageSubscribersDispatcher = messageSubscribersDispatcher;
			_messageDistributor = messageDistributor;

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
			foreach (var o in _startables)
				o.Stop();
		}
	}
}