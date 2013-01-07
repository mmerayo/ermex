// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;


using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Workers
{
    internal class IncomingMessagesProcessorWorker : Worker, IIncomingMessagesProcessorWorker
    {

        [Inject]
        public IncomingMessagesProcessorWorker(IBusSettings settings,
                                               IIncomingMessageSuscriptionsDataSource suscriptionsDataSource,
                                               IIncomingMessagesDataSource messagesDatasource,
                                               IAppComponentDataSource componentDataSource,
            IIncomingMessagesDispatcherWorker dispatcherWorker,IBusMessageDataSource busMessageDataSource)
            : base("IncomingMessagesProcessorWorker")
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (suscriptionsDataSource == null) throw new ArgumentNullException("suscriptionsDataSource");
            if (messagesDatasource == null) throw new ArgumentNullException("messagesDatasource");
            if (componentDataSource == null) throw new ArgumentNullException("componentDataSource");
            if (dispatcherWorker == null) throw new ArgumentNullException("dispatcherWorker");
            if (busMessageDataSource == null) throw new ArgumentNullException("busMessageDataSource");
            Settings = settings;
            SuscriptionsDataSource = suscriptionsDataSource;
            MessagesDatasource = messagesDatasource;
            ComponentDataSource = componentDataSource;
            DispatcherWorker = dispatcherWorker;
            BusMessageDataSource = busMessageDataSource;
        }

        private IBusSettings Settings { get; set; }
        private IIncomingMessageSuscriptionsDataSource SuscriptionsDataSource { get; set; }
        private IIncomingMessagesDataSource MessagesDatasource { get; set; }
        private IAppComponentDataSource ComponentDataSource { get; set; }
        private IIncomingMessagesDispatcherWorker DispatcherWorker { get; set; }
        private IBusMessageDataSource BusMessageDataSource { get; set; }
        private readonly object _SyncLock=new object();
        #region IIncomingMessagesProcessorWorker Members

        protected override void DoWork(object data)
        {
            lock (_SyncLock)
            {
                var messagesToDispatch = BusMessageDataSource.GetMessagesToDispatch();

                foreach (var message in messagesToDispatch)
                {
                    //deserialize

                    Logger.Trace(x => x("{0} - Start Processing from {1}", message.MessageId, message.Publisher));

                    //update component Latency
                    //TODO: FROM TRANSPORT LAYER ?? check correctness as the meaning time is this
                    DateTime receivedTimeUtc = DateTime.UtcNow;
                    UpdateComponentLatency(message, receivedTimeUtc);

                    //get internal suscriptions
                    //TODO: EXTRACT FROM THE JSON, see next line
                    BizMessage bizMessage = BizMessage.FromJson(message.JsonMessage);
                        
                    var suscriptions = GetSubscriptions(bizMessage.MessageType.FullName);

                    foreach (var suscription in suscriptions)
                    {
                        BusMessageData currentBusMessage = BusMessageData.NewFromExisting(message);
                        currentBusMessage.Status=BusMessageData.BusMessageStatus.ReceiverDispatchable;
                        BusMessageDataSource.Save(currentBusMessage);

                        //create an object per suscription    
                        var incomingMessage = new IncomingMessage(currentBusMessage)
                            {
                                ComponentOwner = Settings.ComponentId,
                                PublishedTo = Settings.ComponentId,
                                TimeReceivedUtc = receivedTimeUtc,
                                SuscriptionHandlerId = suscription.SuscriptionHandlerId
                            };

                        //WRITE objects to DB in single transaction
                        MessagesDatasource.Save(incomingMessage);

                        Logger.Trace(
                            x =>
                            x("{0} - Created entry for handler {1}", message.MessageId, suscription.HandlerType));
                    }
                    //delete source file
                    BusMessageDataSource.Remove(message);

                    DispatcherWorker.WorkPendingEvent.Set();
                }
            }
        }

       

        #endregion

        private IEnumerable<IncomingMessageSuscription> GetSubscriptions(string typeFullName)
        {
            var result = new List<IncomingMessageSuscription>();
            var types = TypesHelper.GetInheritanceChain(typeFullName,true);

            foreach (var type in types)
            {
                result.AddRange(SuscriptionsDataSource.GetByMessageType(type.FullName));
            }

            return result;
        }

        private void UpdateComponentLatency(BusMessage receivedMessage, DateTime receivedTimeUtc)
        {
            var milliseconds = receivedTimeUtc.Subtract(receivedMessage.CreatedTimeUtc).Milliseconds;
            if (milliseconds <= (Settings.MaxDelayDueToLatencySeconds*1000))
            {
                ComponentDataSource.UpdateRemoteComponentLatency(receivedMessage.Publisher, milliseconds);
            }
        }

    }
}