using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Stateless;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers;
using ermeX.Bus.Publishing.Dispatching.Messages;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Component;

namespace ermeX.ComponentServices.LocalComponent
{
	internal sealed class LocalComponent:ILocalComponent
	{
		private static readonly ILog Logger = LogManager.GetLogger<LocalComponent>();
		private readonly LocalComponentStateMachine _stateMachine;

		public LocalComponent(IBizSettings settings)
		{
			_stateMachine = new LocalComponentStateMachine(new LocalComponentStateMachineHandler(this,settings));
		}

		public void Start()
		{
			Logger.Debug("Start");
			_stateMachine.Start();
		}


		private sealed class LocalComponentStateMachineHandler:ILocalStateMachinePayloader
		{
			private readonly LocalComponent _localComponent;
			private readonly IBizSettings _bizSettings;
			private readonly IBusSettings _busSettings;
			private readonly IRegisterComponents _componentsRegister;
			private readonly IMessageListener _messageListener;
			private readonly IMessagePublisher _messagePublisher;
			private readonly IReceptionMessageDistributor _receptionMessageDistributor;
			private readonly IMessageSubscribersDispatcher _messageSubscribersDispatcher;
			private readonly IMessageDistributor _messageDistributor;
			private readonly IRegisterComponents _componentsRegistrator;

			public LocalComponentStateMachineHandler(LocalComponent localComponent, 
				IBizSettings bizSettings,
				 IRegisterComponents componentsRegister,
				IMessageListener messageListener,
				IMessagePublisher messagePublisher,
				IReceptionMessageDistributor receptionMessageDistributor,
				IMessageSubscribersDispatcher messageSubscribersDispatcher,
				IMessageDistributor messageDistributor,
				IBusSettings busSettings, 
				IRegisterComponents componentsRegistrator)
			{
				_localComponent = localComponent;
				_bizSettings = bizSettings;
				_componentsRegister = componentsRegister;
				_messageListener = messageListener;
				_messagePublisher = messagePublisher;
				_receptionMessageDistributor = receptionMessageDistributor;
				_messageSubscribersDispatcher = messageSubscribersDispatcher;
				_messageDistributor = messageDistributor;
				_busSettings = busSettings;
				_componentsRegistrator = componentsRegistrator;
			}

			public void Reset()
			{
				Logger.Debug("Reset");
				throw new NotImplementedException();
			}

			public void Stop()
			{
				Logger.Debug("Stop");
				throw new NotImplementedException();
			}

			public void Run()
			{
				Logger.Debug("Run");

				//TODO: CREATE COMMAND for all this
				if (_busSettings.FriendComponent != null)
					_componentsRegistrator.CreateRemoteComponent(_busSettings.FriendComponent.ComponentId,
																_busSettings.FriendComponent.Endpoint.Address.ToString(),
																_busSettings.FriendComponent.Endpoint.Port);

				//TODO:get it from the componentmanager and start
				throw new NotImplementedException();
			}

			public void PublishServices()
			{
				Logger.Debug("PublishServices");
				throw new NotImplementedException();
			}

			public void SubscribeToMessages()
			{
				Logger.Debug("SubscribeToMessages");
				throw new NotImplementedException();
			}

			public void OnStart()
			{
				Logger.Debug("OnStart");
				//TODO: TO START HANDLER
				Logger.Trace(x => x("Component: {0} is STARTING", _bizSettings.ComponentId));
				
				_componentsRegister.CreateLocalComponent(_bizSettings.TcpPort);

				_messageListener.Start();
				_messagePublisher.Start();
				_receptionMessageDistributor.Start();
				_messageSubscribersDispatcher.Start();
				_messageDistributor.Start();
				
				Logger.Trace(x => x("Component: {0} has finished STARTING", _bizSettings.ComponentId));
			}

			public void Error()
			{
				Logger.Debug("Error");
				throw new NotImplementedException();
			}
		}

	}
}
