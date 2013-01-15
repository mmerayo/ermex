using System;
using System.Linq;
using System.Text;
using Common.Logging;
using Ninject;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Schedulers;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading.Scheduling;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers
{
    //TODO: IT MUST BE IN PARALLEL, THIS IS TEMPORAL UNTIL iSSUE-244 is developed
    //TODO: THE WHOLE CLAS LOGIC SHOULD BE REWRITTEN
    internal sealed class QueueDispatcherManager:IQueueDispatcherManager
    {
        public class QueueDispatcherManagerMessage
        {
            public IncomingMessage IncomingMessage { get; private set; }
            public bool MustCalculateLatency { get; private set; }

            public QueueDispatcherManagerMessage(IncomingMessage message, bool mustCalculateLatency=true)
            {

                if (message == null) throw new ArgumentNullException("message");
                IncomingMessage = message;
                MustCalculateLatency = mustCalculateLatency;
            }
        }


        [Inject]
        public QueueDispatcherManager(IBusSettings settings, 
            IAppComponentDataSource componentDataSource,
            IIncomingMessagesDataSource messagesDataSource,
            IScheduler scheduler, IJobScheduler jobScheduler)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (componentDataSource == null) throw new ArgumentNullException("componentDataSource");
            if (messagesDataSource == null) throw new ArgumentNullException("messagesDataSource");
            if (scheduler == null) throw new ArgumentNullException("scheduler");
            if (jobScheduler == null) throw new ArgumentNullException("jobScheduler");
            ComponentDataSource = componentDataSource;
            MessagesDataSource = messagesDataSource;
            Scheduler = scheduler;
            JobScheduler = jobScheduler;
            Settings = settings;

            //TODO: REMOVE WHEN ISSUE-244 IS DONE
            JobScheduler.ScheduleJob(Job.At(DateTime.UtcNow.AddSeconds(2),DoDeliver));

        }
        private readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);
        private IBusSettings Settings { get; set; }
        private IAppComponentDataSource ComponentDataSource { get; set; }
        private IIncomingMessagesDataSource MessagesDataSource { get; set; }
        private IScheduler Scheduler { get; set; }
        private IJobScheduler JobScheduler { get; set; }

        public void EnqueueItem(QueueDispatcherManagerMessage message)
        {
            //TODO: IT DOES THE DISPATCH TEMPORALLY, BUT IT MUST RESEND TO EACH QUEUE
            //TODO: we need to IMPLEMENT the RETRIES IN THE FINAL QUEUE ISSUE-244

            IncomingMessage incomingMessage = message.IncomingMessage;
            incomingMessage.Status=Message.MessageStatus.ReceiverDispatching;
            MessagesDataSource.Save(incomingMessage);
            if(message.MustCalculateLatency)
                UpdateComponentLatency(incomingMessage.ToBusMessage(),DateTime.UtcNow);
        }

        //TODO: MOVE TO THE FINAL QUEUE
        private void UpdateComponentLatency(BusMessage receivedMessage, DateTime receivedTimeUtc)
        {
            var milliseconds = receivedTimeUtc.Subtract(receivedMessage.CreatedTimeUtc).Milliseconds;
            if (milliseconds <= (Settings.MaxDelayDueToLatencySeconds * 1000))
            {
                ComponentDataSource.UpdateRemoteComponentLatency(receivedMessage.Publisher, milliseconds);
            }
        }

        private void DoDeliver()
        {
            var item = Scheduler.GetNext();
            if (item != null)
            {

                Logger.Trace(x => x("{0} Start Handling", item.MessageId));
                try
                {
                    OnDispatchMessage(item.SuscriptionHandlerId, item.ToBusMessage());
                }
                catch
                {
                    item.Status = Message.MessageStatus.ReceiverDispatchable; //leaves it in the previous status
                    MessagesDataSource.Save(item);
                    JobScheduler.ScheduleJob(Job.At(DateTime.UtcNow.AddSeconds(2), DoDeliver));
                    return;
                }
                MessagesDataSource.Remove(item);
                Logger.Trace(x => x("{0} Handled finally", item.MessageId));
                JobScheduler.ScheduleJob(Job.At(DateTime.UtcNow.AddSeconds(2), DoDeliver));

            }
        }

        public event Action<Guid, object> DispatchMessage;

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
