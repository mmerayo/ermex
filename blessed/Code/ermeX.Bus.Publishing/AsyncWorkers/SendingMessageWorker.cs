// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;


using ermeX.Entities.Entities;
using ermeX.Exceptions;
using ermeX.LayerMessages;
using ermeX.Threading;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Sending.Client;

namespace ermeX.Bus.Publishing.AsyncWorkers
{
    internal sealed class SendingMessageWorker : Worker, ISendingMessageWorker
    {

        [Inject]
        public SendingMessageWorker(IOutgoingMessagesDataSource dataSource,IBusMessageDataSource busMessageDataSource,
                                    IBusSettings clientConfiguration, IServiceProxy service)
            : base("SendingMessageWorker")
        {
            if (dataSource == null) throw new ArgumentNullException("dataSource");
            if (busMessageDataSource == null) throw new ArgumentNullException("busMessageDataSource");
            if (clientConfiguration == null) throw new ArgumentNullException("clientConfiguration");
            if (service == null) throw new ArgumentNullException("service");

            DataSource = dataSource;
            BusMessageDataSource = busMessageDataSource;
            Settings = clientConfiguration;
            Service = service;
            
        }

        private IOutgoingMessagesDataSource DataSource { get; set; }
        private IBusMessageDataSource BusMessageDataSource { get; set; }
        private IBusSettings Settings { get; set; }

        private IServiceProxy Service { get; set; }

        #region ISendingMessageWorker Members

       
        protected override void DoWork(object data)
        {
            if (data == null) throw new ArgumentNullException("data");
            //string _serializedMessagePath;

            var messageToSend = (OutgoingMessage)data;
            
            if (messageToSend.Failed) return; //TODO: THE CALLER SHOULD NEVER SEND THIS MESSAGE HERE

            BusMessageData busMessage = BusMessageDataSource.GetById(messageToSend.BusMessageId);
           
            if ((DateTime.UtcNow - messageToSend.TimePublishedUtc) > Settings.SendExpiringTime) //TODO: NEVER EXPIRES PLEASE, MUST BE REMOVED MANUALLY FROM UTILITY
            {
                Logger.Warn(x=>x("FATAL! {0} not sent to {1} AND EXPIRED", busMessage.MessageId, messageToSend.PublishedTo));
                messageToSend.Failed = true;
                ReserializeMessage(messageToSend, busMessage);
            }
            else
            {
                if (SendData(messageToSend,busMessage))
                {
                    RemoveRecord(messageToSend,busMessage);
                    Logger.Trace(x=>x("SUCCESS {0} Sent to {1}", busMessage.MessageId, messageToSend.PublishedTo));
                }
                else
                {
                    messageToSend.Tries += 1;

                    messageToSend.Delivering = false;
                    ReserializeMessage(messageToSend,busMessage);
                    Logger.Trace(x=>x("FAILED! {0} not sent to {1}", busMessage.MessageId, messageToSend.PublishedTo));
                }
            }
        }


        #endregion
        

        private void ReserializeMessage(OutgoingMessage messageToSend, BusMessageData busMessage)
        {
            busMessage.Status=BusMessageData.BusMessageStatus.SenderDispatchPending;
            BusMessageDataSource.Save(busMessage);
            DataSource.Save(messageToSend);
        }

     

        private void RemoveRecord(OutgoingMessage messageToSend, BusMessageData busMessage)
        {
            Debug.Assert(messageToSend.BusMessageId>0 && messageToSend.BusMessageId==busMessage.Id);
            BusMessageDataSource.Remove(busMessage);//TODO: improve 
            DataSource.Remove(messageToSend);
        }


        private bool SendData(OutgoingMessage data,BusMessageData busMessage)
        {

            var transportMessage = new TransportMessage(data.PublishedTo, busMessage);

            ServiceResult serviceResult;
            try
            {
                serviceResult = Service.Send(transportMessage);
            }catch(ermeXComponentNotAvailableException ex)
            {
                Logger.Warn(x=>x("Couldn't send message {0} to {1}.{2}",busMessage.MessageId, data.PublishedTo,ex));
                return false;
            }
            if (!serviceResult.Ok)
            {
                if(serviceResult.ServerMessages==null || serviceResult.ServerMessages.Count==0)
                    throw new ApplicationException("The service didnt return an error meassage at least. It is expected.");

                string logData = serviceResult.ServerMessages.Aggregate("Server returned errors: ", (current, serverMessage) => current + Environment.NewLine +  serverMessage );

                Logger.Error(x=>x("{0}",logData));
            }

            busMessage.Status=BusMessageData.BusMessageStatus.SenderSent;
            BusMessageDataSource.Save(busMessage);

            return serviceResult.Ok;
        }
    }
}