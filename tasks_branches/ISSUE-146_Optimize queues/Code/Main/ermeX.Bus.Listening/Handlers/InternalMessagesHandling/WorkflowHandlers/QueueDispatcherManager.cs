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
            ComponentsDataSource = componentDataSource;
            MessagesDataSource = messagesDataSource;
            Scheduler = scheduler;
            JobScheduler = jobScheduler;
            Settings = settings;
            NextScheduledDelivery = DateTime.MaxValue;
        }
        private readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);
        private IBusSettings Settings { get; set; }
        private IAppComponentDataSource ComponentsDataSource { get; set; }
        private IIncomingMessagesDataSource MessagesDataSource { get; set; }
        private IScheduler Scheduler { get; set; }
        private IJobScheduler JobScheduler { get; set; }
        private DateTime NextScheduledDelivery { get; set; }
        private readonly object _scheduleLocker=new object();
        private readonly object _locker = new object();

        public void EnqueueItem(QueueDispatcherManagerMessage message)
        {
            //TODO: IT DOES THE DISPATCH TEMPORALLY, BUT IT MUST RESEND TO EACH QUEUE
            //TODO: we need to IMPLEMENT the RETRIES IN THE FINAL QUEUE ISSUE-244

            IncomingMessage incomingMessage = message.IncomingMessage;
            incomingMessage.Status=Message.MessageStatus.ReceiverDispatching;
            MessagesDataSource.Save(incomingMessage);
            if(message.MustCalculateLatency)
                UpdateComponentLatency(incomingMessage.ToBusMessage(),DateTime.UtcNow);

            //ScheduleDelivery
            int maxLatency = ComponentsDataSource.GetMaxLatency();
            DateTime nextScheduleCandidate = DateTime.UtcNow.AddMilliseconds(maxLatency);
            TryScheduleDelivery(nextScheduleCandidate);
        }

        private void TryScheduleDelivery(DateTime nextScheduleCandidate)
        {
            lock(_scheduleLocker)
            {
                if(nextScheduleCandidate < NextScheduledDelivery) //Add to scheduler
                {
                    NextScheduledDelivery = nextScheduleCandidate;
                    JobScheduler.ScheduleJob(Job.At(nextScheduleCandidate, DoDeliver));
                }
            }
        }

        //TODO: MOVE TO THE FINAL QUEUE
        private void UpdateComponentLatency(BusMessage receivedMessage, DateTime receivedTimeUtc)
        {
            var milliseconds = receivedTimeUtc.Subtract(receivedMessage.CreatedTimeUtc).Milliseconds;
            if (milliseconds <= (Settings.MaxDelayDueToLatencySeconds * 1000))
            {
                ComponentsDataSource.UpdateRemoteComponentLatency(receivedMessage.Publisher, milliseconds);
            }
        }

        private void DoDeliver()
        {
            if (_disposed) return;

            lock (_scheduleLocker)
            {
                NextScheduledDelivery = DateTime.MaxValue;
            }
            lock (_locker)
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
                        TryScheduleDelivery(DateTime.UtcNow.AddSeconds(2));
                            //it will continue rescheduling while there are more
                        return;
                    }
                    MessagesDataSource.Remove(item);
                    Logger.Trace(x => x("{0} Handled finally", item.MessageId));
                    TryScheduleDelivery(DateTime.UtcNow.AddSeconds(2));
                        //it will continue rescheduling while there are more
                }
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
                throw;
            }
        }

        #region IDisposable

        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
        }
        private void Dispose(bool disposing)
        {
            JobScheduler.RemoveJobsByAction(DoDeliver);
            if(disposing)
            {
                DispatchMessage = null;
            }
            _disposed = true;
        }

        ~QueueDispatcherManager()
        {
            Dispose(false);
        }
        #endregion
    }
}
