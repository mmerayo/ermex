// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.Interfaces;

using ermeX.Entities.Entities;

namespace ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByService
{
    internal sealed class MessageSuscriptionsRequestMessageHandler : IMessageSuscriptionsService
    {
        [Inject]
        public MessageSuscriptionsRequestMessageHandler(IMessagePublisher publisher,
                                                        IMessageListener listener, IComponentSettings settings,
                                                        IIncomingMessageSuscriptionsDataSource
                                                            incomingMessageSuscriptionsDataSource,
                                                        IOutgoingMessageSuscriptionsDataSource
                                                            outgoingMessageSuscriptionsDataSource, 
            IStatusManager statusManager)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            if (settings == null) throw new ArgumentNullException("settings");
            if (incomingMessageSuscriptionsDataSource == null)
                throw new ArgumentNullException("incomingMessageSuscriptionsDataSource");
            if (outgoingMessageSuscriptionsDataSource == null)
                throw new ArgumentNullException("outgoingMessageSuscriptionsDataSource");
            if (statusManager == null) throw new ArgumentNullException("statusManager");
            Publisher = publisher;
            Listener = listener;
            Settings = settings;
            IncomingMessageSuscriptionsDataSource = incomingMessageSuscriptionsDataSource;
            OutgoingMessageSuscriptionsDataSource = outgoingMessageSuscriptionsDataSource;
            StatusManager = statusManager;
        }

        private IMessagePublisher Publisher { get; set; }

        private IMessageListener Listener { get; set; }
        private IComponentSettings Settings { get; set; }
        private IIncomingMessageSuscriptionsDataSource IncomingMessageSuscriptionsDataSource { get; set; }
        private IOutgoingMessageSuscriptionsDataSource OutgoingMessageSuscriptionsDataSource { get; set; }
        private readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);
        private IStatusManager StatusManager { get; set; }

        #region IMessageSuscriptionsService Members

        public MessageSuscriptionsResponseMessage RequestSuscriptions(MessageSuscriptionsRequestMessage request)
        {
            var result = new MessageSuscriptionsResponseMessage(Settings.ComponentId, request.CorrelationId)
                             {
                                 MyIncomingSuscriptions = IncomingMessageSuscriptionsDataSource.GetAll(),
                                 MyOutgoingSuscriptions =
                                     OutgoingMessageSuscriptionsDataSource.GetAll().Where(
                                         x => x.Component != request.SourceComponentId).ToList()
                             };

            return result;
        }

        public void AddSuscription(IncomingMessageSuscription request)
        {
            StatusManager.WaitIsRunning();
            try
            {
                if (request == null) throw new ArgumentNullException("request");
                OutgoingMessageSuscriptionsDataSource.SaveFromOtherComponent(request);
            }
            catch (Exception ex)
            {
                Logger.Warn(x=>x("Could not handle request", ex));
            }
        }



        public void AddSuscriptions(IList<IncomingMessageSuscription> request)
        {
            try
            {
                StatusManager.WaitIsRunning();
                foreach (var incomingMessageSuscription in request)
                {
                    AddSuscription(incomingMessageSuscription);
                }
            }
            catch(Exception ex)
            {
                Logger.Warn(x=>x("Could not handle request", ex));
            }
        }

        #endregion
    }
}