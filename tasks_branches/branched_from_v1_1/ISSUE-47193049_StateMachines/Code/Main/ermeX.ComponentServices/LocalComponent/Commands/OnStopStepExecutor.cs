using System;
using System.Collections.Generic;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers;
using ermeX.Bus.Publishing.Dispatching.Messages;
using ermeX.DAL.Interfaces.Component;

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
		private IEnumerable<object> _toDispose;


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

			_toDispose = new List<object>()
				{
					componentsRegister,
					messageListener,
					messagePublisher,
					receptionMessageDistributor,
					messageSubscribersDispatcher,
					messageDistributor
				};


		}

		public void Stop()
		{
			foreach (var o in _toDispose)
			{
				var disposable = o as IDisposable;
				if(disposable!=null)
					disposable.Dispose();
			}
		}
	}
}