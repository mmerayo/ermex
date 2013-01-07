// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Ninject;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Schedulers;
using ermeX.Common;
using ermeX.DAL.Interfaces;


using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Workers
{
    //TODO: THIS SHOULD BE DONE BY THE BIZ LAYER OR TO BE PASSED TO THE bIZ LAYER, WHERE DELIVERY TO USER AND OR INVOCATES HANDLERS
    internal class IncomingMessagesSyncDispatcherWorker :Worker, IIncomingMessagesDispatcherWorker
    {

        [Inject]
        public IncomingMessagesSyncDispatcherWorker(IIncomingMessagesDataSource messagesDataSource,
            IBusMessageDataSource busMessageDataSource, IScheduler scheduler)
            : base("IncomingMessagesSyncDispatcherWorker", new TimeSpan(0, 0, 1))
        //TODO, THIS VALUE TO BE CONFIGURABLE AND CHANGE FOR TESTING
        {
            if (messagesDataSource == null) throw new ArgumentNullException("messagesDataSource");
            if (busMessageDataSource == null) throw new ArgumentNullException("busMessageDataSource");
            if (scheduler == null) throw new ArgumentNullException("scheduler");
            MessagesDataSource = messagesDataSource;
            BusMessageDataSource = busMessageDataSource;
            Scheduler = scheduler;
        }

        private IIncomingMessagesDataSource MessagesDataSource { get; set; }
        private IBusMessageDataSource BusMessageDataSource { get; set; }
        private IScheduler Scheduler { get; set; }

        #region IIncomingMessagesDispatcherWorker Members

        protected override void DoWork(object data)
        {
            var item = Scheduler.GetNext();
            if (item != null)
            {
                var busMessage = BusMessageDataSource.GetById(item.BusMessageId);

                Logger.Trace(x=>x("{0} Start Handling", busMessage.MessageId));
                try
                {
                    OnDispatchMessage(item.SuscriptionHandlerId, busMessage);
                }
                catch
                {
                    busMessage.Status=BusMessageData.BusMessageStatus.ReceiverDispatchable;
                    BusMessageDataSource.Save(busMessage);
                    throw;
                }
                
                BusMessageDataSource.Remove(busMessage);
                MessagesDataSource.Remove(item);
                Logger.Trace(x=>x("{0} Handled finally",busMessage.MessageId));
            }
        }

        public event Action<Guid, object> DispatchMessage;

        #endregion

        //TODO: invocation must be done on the biz layer
        private void OnDispatchMessage(Guid suscriptionHandlerId, BusMessage message)
        {
            try
            {
                Action<Guid, object> handler = DispatchMessage;
                if (handler != null)
                {
                    handler(suscriptionHandlerId, message.Data.RawData);
                    Logger.Trace(x=>x("Receiver: Dispatched message with id: {0}", message.MessageId));
                }
                else
                {
                    Logger.Trace(x=>x("Receiver: The message with id: {0} didnt have receivers configured", message.MessageId));
                }

            }catch(Exception ex)
            {
                Logger.Error(x=>x("Error handling message {0} by subscriptionHandler {1}", message.MessageId, suscriptionHandlerId),ex);
                throw ex;
            }
        }

        
    }
}