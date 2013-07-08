
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers;
using ermeX.Bus.Publishing.Dispatching.Messages;
using ermeX.ComponentServices.Interfaces.LocalComponent.Commands;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Component;
using ermeX.Logging;

namespace ermeX.ComponentServices.LocalComponent.Commands
{
	internal class OnStartStepExecutor : IOnStartStepExecutor
	{
		private readonly ILogger Logger;

		private readonly IBizSettings _bizSettings;
		private readonly IRegisterComponents _componentsRegister;
		private readonly IMessageListener _messageListener;
		private readonly IMessagePublisher _messagePublisher;
		private readonly IReceptionMessageDistributor _receptionMessageDistributor;
		private readonly IMessageSubscribersDispatcher _messageSubscribersDispatcher;
		private readonly IMessageDistributor _messageDistributor;

		[Inject]
		public OnStartStepExecutor(IBizSettings bizSettings, 
		                                IRegisterComponents componentsRegister,
		                                IMessageListener messageListener, 
		                                IMessagePublisher messagePublisher, 
		                                IReceptionMessageDistributor receptionMessageDistributor,
		                                IMessageSubscribersDispatcher messageSubscribersDispatcher, 
		                                IMessageDistributor messageDistributor,IComponentSettings settings)
		{
			Logger = LogManager.GetLogger<OnStartStepExecutor>(settings.ComponentId, LogComponent.Handshake);
			_bizSettings = bizSettings;
			_componentsRegister = componentsRegister;
			_messageListener = messageListener;
			_messagePublisher = messagePublisher;
			_receptionMessageDistributor = receptionMessageDistributor;
			_messageSubscribersDispatcher = messageSubscribersDispatcher;
			_messageDistributor = messageDistributor;
		}


		public void DoStart()
		{
			Logger.Trace(x => x("DoStart- {0} is STARTING", _bizSettings.ComponentId));

			_componentsRegister.CreateLocalComponent(_bizSettings.TcpPort);

			_messageListener.Start();
			_messagePublisher.Start();
			_receptionMessageDistributor.Start();
			_messageSubscribersDispatcher.Start();
			_messageDistributor.Start();
		}
	}
}