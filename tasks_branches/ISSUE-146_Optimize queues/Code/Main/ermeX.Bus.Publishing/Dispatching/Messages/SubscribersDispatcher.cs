using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;
using ermeX.Exceptions;
using ermeX.LayerMessages;
using ermeX.Threading.Queues;
using ermeX.Threading.Scheduling;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Sending.Client;

namespace ermeX.Bus.Publishing.Dispatching.Messages
{
    /// <summary>
    /// Sends the messages and handle sending failures
    /// </summary>
    sealed class SubscribersDispatcher : ProducerParallelConsumerQueue<SubscribersDispatcher.SubscribersDispatcherMessage>, ISubscribersDispatcher
    {
       
        private const int _maxThreadsNum = 64;
        private const int _queueSizeToCreateNewThread = 4;
        private const int _initialWorkerCount = 4;

        public class SubscribersDispatcherMessage{
            public OutgoingMessage OutGoingMessage { get; private set; }

            public SubscribersDispatcherMessage(OutgoingMessage outGoingMessage)
            {
                if (outGoingMessage == null) throw new ArgumentNullException("outGoingMessage");
                OutGoingMessage = outGoingMessage;
            }
        }

        [Inject]
        public SubscribersDispatcher(IBusSettings settings, IOutgoingMessagesDataSource dataSource, IJobScheduler taskScheduler, SystemTaskQueue taskQueue, IServiceProxy service)
            : base(_initialWorkerCount, _maxThreadsNum, _queueSizeToCreateNewThread, TimeSpan.FromSeconds(60))
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (dataSource == null) throw new ArgumentNullException("dataSource");
            if (taskScheduler == null) throw new ArgumentNullException("taskScheduler");
            if (taskQueue == null) throw new ArgumentNullException("taskQueue");
            if (service == null) throw new ArgumentNullException("service");
            Settings = settings;
            DataSource = dataSource;
            JobScheduler = taskScheduler;
            TaskQueue = taskQueue;
            Service = service;

            RestoreMessagesFromPreviousSessions();
        }

        private IBusSettings Settings { get; set; }
        private IOutgoingMessagesDataSource DataSource { get; set; }
        private IJobScheduler JobScheduler { get; set; }
        private SystemTaskQueue TaskQueue { get; set; }
        private IServiceProxy Service { get; set; }

        protected override Action<SubscribersDispatcherMessage> RunActionOnDequeue
        {
            get { return OnDequeue; }
        }

        private void OnDequeue(SubscribersDispatcherMessage obj)
        {
            var messageToSend = obj.OutGoingMessage;
            if (messageToSend.Status!=Message.MessageStatus.SenderDispatchPending) 
                return;

            if (messageToSend.Expired(Settings.SendExpiringTime))
            {
                messageToSend.Status = Message.MessageStatus.SenderFailed;
                Logger.Warn(x => x("FATAL! {0} not sent to {1} AND EXPIRED.It wont be retried", messageToSend.MessageId, messageToSend.PublishedTo));
                TaskQueue.EnqueueItem(()=>DataSource.Save(messageToSend));
                return;
            }

            if (SendData(messageToSend))
            {
                messageToSend.Status=Message.MessageStatus.SenderSent;
                TaskQueue.EnqueueItem(() => DataSource.Save(messageToSend));
                Logger.Info(x => x("SUCCESS {0} Sent to {1}", messageToSend.MessageId, messageToSend.PublishedTo));
            }
            else
            {
                messageToSend.Tries += 1;

                TaskQueue.EnqueueItem(() => DataSource.Save(messageToSend));
                TaskQueue.EnqueueItem(()=>ReEnqueueItem(messageToSend));
                Logger.Warn(x => x("FAILED! {0} not sent to {1}. It will be retried", messageToSend.MessageId, messageToSend.PublishedTo));
            }

        }

        private bool SendData(OutgoingMessage data)
        {
            BusMessage busMessage = data.ToBusMessage();
            var transportMessage = new TransportMessage(data.PublishedTo, busMessage);

            ServiceResult serviceResult;
            try
            {
                serviceResult = Service.Send(transportMessage);
            }
            catch (Exception ex)
            {
                Logger.Error(x => x("Couldn't send message {0} to {1}.{2}", busMessage.MessageId, data.PublishedTo, ex));
                return false;
            }
            if (!serviceResult.Ok)
            {
                if (serviceResult.ServerMessages == null || serviceResult.ServerMessages.Count == 0)
                    throw new ApplicationException("The service didnt return an error meassage at least. It is expected.");

                string logData = serviceResult.ServerMessages.Aggregate("Server returned errors: ", (current, serverMessage) => current + Environment.NewLine + serverMessage);

                Logger.Error(x => x("{0}", logData));
                
            }
            
            return serviceResult.Ok;
        }

        private void ReEnqueueItem(OutgoingMessage messageToSend)
        {
            if (messageToSend == null) throw new ArgumentNullException("messageToSend");
            
            //wait 5*tries seconds until a max of 2 minutes and retry send message
            int suggestedRetryTime = messageToSend.Tries * 5;
            int fireTime = suggestedRetryTime > 120 ? 120 : suggestedRetryTime;
         
            var job = Job.At(DateTime.UtcNow.AddSeconds(fireTime),
                             () => EnqueueItem(new SubscribersDispatcherMessage(messageToSend)));
            
            JobScheduler.ScheduleJob(job);
        }


        private void RestoreMessagesFromPreviousSessions()
        {
            var outgoingMessages = DataSource.GetByStatus(Message.MessageStatus.SenderDispatchPending);

            foreach (var outgoingMessage in outgoingMessages)
            {
                ReEnqueueItem(outgoingMessage);
            }
        }
    }
}