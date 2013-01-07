// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Bus.Listening
{
    internal class EsbManager : IEsbManager, IDisposable
    {
        [Inject]
        public EsbManager(IBusSettings settings, IMessagePublisherDispatcherStrategy messagesDispatcher,
                          IServiceRequestDispatcher serviceRequestDispatcher, IServiceDetailsDataSource dataSource)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (messagesDispatcher == null) throw new ArgumentNullException("messagesDispatcher");
            if (serviceRequestDispatcher == null) throw new ArgumentNullException("serviceRequestDispatcher");
            if (dataSource == null) throw new ArgumentNullException("dataSource");
            Settings = settings;

            MessagesDispatcher = messagesDispatcher;
            ServiceRequestDispatcher = serviceRequestDispatcher;
            DataSource = dataSource;
        }

        private IMessagePublisherDispatcherStrategy MessagesDispatcher { get; set; }

        private IBusSettings Settings { get; set; }

        private IServiceRequestDispatcher ServiceRequestDispatcher { get; set; }
        private IServiceDetailsDataSource DataSource { get; set; }

        #region IEsbManager Members

        public void Publish(BusMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            //var messageToPublish = GetPublisheableMessage(message);
             
            MessagesDispatcher.Dispatch(message);
        }

        public void Start()
        {
            lock (this)
            {
                if (MessagesDispatcher.Status == DispatcherStatus.Stopped)
                    MessagesDispatcher.Start();
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        #endregion

        //private OutgoingMessage GetPublisheableMessage(object message)
        //{
        //    cambiar esto para que use el bus message para metener estos datos guardandolos en la bbdd indicando la direccion

        //    var result = new OutgoingMessage(message)
        //                     {
        //                         PublishedBy = Settings.ComponentId,
        //                         TimePublishedUtc = DateTime.UtcNow
        //                     };
        //    return result;
        //}

        #region Services

        public IServiceOperationResult<TResult> RequestService<TResult>(Guid destinationComponent, Guid serviceOperation,
                                                                        object[] requestParams)
        {
            if (destinationComponent.IsEmpty()) throw new ArgumentException();
            ServiceRequestMessage request = GetServiceRequestMessage<TResult>(destinationComponent, serviceOperation,
                                                                              requestParams);
            return ServiceRequestDispatcher.RequestSync<TResult>(request);
        }


        public void RequestService<TResult>(Guid destinationComponent, Guid serviceOperation,
                                            Action<IServiceOperationResult<TResult>> responseHandler,
                                            object[] requestParams)
        {
            if (destinationComponent.IsEmpty()) throw new ArgumentException();
            //TODO: ASYNC FUNCTIONALLITY
            ServiceRequestMessage request = GetServiceRequestMessage<TResult>(destinationComponent, serviceOperation,
                                                                              requestParams);
            ServiceRequestDispatcher.RequestAsync(request, responseHandler);
        }

        private ServiceRequestMessage GetServiceRequestMessage<TResult>(Guid destinationComponent, Guid operationId,
                                                                        object[] requestParams)
        {
            Guid serverId = destinationComponent != Guid.Empty
                                ? destinationComponent
                                : DataSource.GetByOperationId(operationId).Publisher;

            var callingContextId = typeof (TResult) != typeof (void) ? Guid.NewGuid() : default(Guid);

            //TODO: MUST CHANGE WHEN DEVELOPING CALLER, TO PROVIDE THE CORRECT PARAM NAMES FROM A PROXY COMPILED ON THE FLY, change this interface to provide the parameters and reorganize class etc..... THE NEXT LINEs ONLY COMPILES

            var requestParameters = new Dictionary<string, object>();
            int i = 0;

            foreach (var requestParam in requestParams)
            {
                requestParameters.Add("Param" + i++, requestParam);
            }

            //finished crap TODO
            var result = ServiceRequestMessage.GetForServiceRequest<TResult>(serverId, operationId, callingContextId,
                                                                             requestParameters);

            return result;
        }

        #endregion

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    MessagesDispatcher.Dispose();
                    MessagesDispatcher = null;
                }

                _disposed = true;
            }
        }

        #endregion
    }
}