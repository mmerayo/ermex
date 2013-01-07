// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Configuration;
using Ninject.Modules;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.Bus.Listening;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Schedulers;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Workers;
using ermeX.Bus.Publishing;
using ermeX.Bus.Publishing.AsyncWorkers;
using ermeX.Bus.Publishing.ClientProxies;
using ermeX.Bus.Publishing.Dispatching;
using ermeX.Bus.Synchronisation;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;

namespace ermeX.Bus.IoC
{
    internal class BusInjections : NinjectModule
    {
        private readonly IBusSettings _settings;

        public BusInjections(IBusSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            List<string> errors;
            if (!new BusSettingsValidator(settings).Validate(out errors))
            {
                throw new ConfigurationException(string.Join(",", errors));
            }
            _settings = settings;
        }

        public override void Load()
        {
            Bind<IEsbManager>().To<EsbManager>().InSingletonScope();
            //TODO: CHANGE THIS when configuration has values passing the whole section
            Bind<IBusSettings>().ToConstant(_settings);

            //TODO:
            Bind<IMessageListener>().To<MessageListeningManager>().InSingletonScope();
            Bind<IListeningManager>().To<ListeningManager>().InSingletonScope();
            //if (_settings.Publisher != null)
            //    Bind<IMessagePublisher>().ToConstant(_settings.Publisher);
            //else
                Bind<IMessagePublisher>().To<MessagePublishingManager>().InSingletonScope();
            Bind<IListeningStrategyProvider>().To<ListeningStrategyProvider>().InSingletonScope();
            Bind<IServiceRequestManager>().To<ServiceRequestManager>().InSingletonScope();
            Bind<IServiceRequestDispatcher>().To<ServiceRequestDispatcher>().InSingletonScope();
            Bind<IServiceCallsProxy>().To<ServiceCallsProxy>();
            Bind<IMessagePublisherDispatcherStrategy>().To<MessageDispatcher>().InSingletonScope();
            Bind<IIncomingMessagesProcessorWorker>().To<IncomingMessagesProcessorWorker>().InSingletonScope();
            Bind<IIncomingMessagesDispatcherWorker>().To<IncomingMessagesSyncDispatcherWorker>().InSingletonScope();
            Bind<IScheduler>().To<IncommingMessagesFifoScheduler>().InSingletonScope();
            Bind<ISendingMessageWorker>().To<SendingMessageWorker>();
        }
    }
}