// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Common.Logging;
using Ninject;
using ermeX.Common.Caching;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;


using ermeX.Entities.Entities;
using ermeX.Exceptions;
using ermeX.LayerMessages;
using ermeX.Transport.Interfaces;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Messages.ServiceOperations;
using ermeX.Transport.Interfaces.Sending.Client;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Transport.Publish
{
    /// <summary>
    ///   Chooses the best option for sending the message
    /// </summary>
    internal class ServiceProxy : IServiceProxy
    {
        private readonly object _cacheProviderLocker = new object();

        [Inject]
        internal ServiceProxy(ICacheProvider cacheProvider,
                              IConnectivityManager connectivityManager, ITransportSettings settings)
        {
            if (cacheProvider == null) throw new ArgumentNullException("cacheProvider");
            if (connectivityManager == null) throw new ArgumentNullException("connectivityManager");
            if (settings == null) throw new ArgumentNullException("settings");
            
            CacheProvider = cacheProvider;
            ConnectivityManager = connectivityManager;
            Settings = settings;
            
        }
        private ICacheProvider CacheProvider { get; set; }
        private IConnectivityManager ConnectivityManager { get; set; }
        private ITransportSettings Settings { get; set; }
	    private static readonly ILog Logger = LogManager.GetLogger(typeof (ServiceProxy).FullName);

        #region IServiceProxy Members

        public ServiceResult Send(TransportMessage message)
        {
			Logger.TraceFormat("Send. message: {0}",message.Data.Data.JsonMessage);

            var realMsg =
                ServiceRequestMessage.GetForMessagePublishing(message);

            ServiceResult result = DoSend(message.Recipient, realMsg);
            return result;
        }

        public ServiceOperationResult<TResult> SendServiceRequestSync<TResult>(IOperationServiceRequestData request)
        {
            Logger.Info(x=>x("Sending Sync service request {0}",request.Operation));
            var partialResult = DoSend(request.ServerId, request as ServiceRequestMessage);
            var result = new ServiceOperationResult<TResult>(partialResult);
            //TODO:remove this conversion and unify types from ServiceResult to ServiceOperationResult
            return result;
        }

        public void SendServiceRequestAsync<TResult>(IOperationServiceRequestData request,
                                                     Action<IServiceOperationResult<TResult>> responseHandler)
        {
            Logger.Info(x=>x("Sending Async service request {0}", request.Operation));
            DoSend(request.ServerId, request as ServiceRequestMessage, responseHandler);
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
                    //TODO
                    CacheProvider.Remove(PreselectedEndPointsKey);
                    foreach (var endpoints in Endpoints.Values)
                    {
                        foreach (var endPoint in endpoints)
                        {
                            endPoint.Dispose();
                        }
                    }
                    CacheProvider.Remove(EndPointsKey);
                }

                _disposed = true;
            }
        }

        #endregion

        #region Private

        //cache provider will enforce refresh connection
        private static readonly object _endPointLocker = new object();
        private static readonly object _preselectedEndPointLocker = new object();

        private string EndPointsKey
        {
            get { return string.Format("Ed{0}", Settings.ComponentId); }
        }

        private string PreselectedEndPointsKey
        {
            get { return string.Format("PePk{0}", Settings.ComponentId); }
        }


        private IDictionary<Guid, IList<IEndPoint>> Endpoints
        {
            get
            {
                if (!CacheProvider.Contains(EndPointsKey))
                    lock (_cacheProviderLocker)
                        if (!CacheProvider.Contains(EndPointsKey))
                            CacheProvider.Add(EndPointsKey, new ConcurrentDictionary<Guid, IList<IEndPoint>>());
                return CacheProvider.Get<ConcurrentDictionary<Guid, IList<IEndPoint>>>(EndPointsKey);
            }
            set
            {
                if (!CacheProvider.Contains(EndPointsKey))
                    lock (_cacheProviderLocker)
                        if (!CacheProvider.Contains(EndPointsKey))
                            CacheProvider.Add(EndPointsKey, value);
            }
        }

        private IDictionary<Guid, int> PreselectedEndPoints
        {
//TODO: CHANGE THIS CRAPPY 
            get
            {
                if (!CacheProvider.Contains(PreselectedEndPointsKey))
                    lock (_cacheProviderLocker)
                        if (!CacheProvider.Contains(PreselectedEndPointsKey))
                            CacheProvider.Add(PreselectedEndPointsKey, new ConcurrentDictionary<Guid, int>());
                return CacheProvider.Get<ConcurrentDictionary<Guid, int>>(PreselectedEndPointsKey);
            }
            set
            {
                if (!CacheProvider.Contains(PreselectedEndPointsKey))
                    lock (_cacheProviderLocker)
                        if (!CacheProvider.Contains(PreselectedEndPointsKey))
                            CacheProvider.Add(PreselectedEndPointsKey, value);
            }
        }

        private ServiceResult DoSend(IEndPoint endPoint, ServiceRequestMessage realMsg)
        {
            try
            {
                return endPoint.Send(realMsg);
            }
            catch (ermeXComponentNotAvailableException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                //TODO:LOG EXCEPTION
                var serviceResult = new ServiceResult(false);
                serviceResult.ServerMessages.Add(string.Format("{0} Inner exception:{1}", ex.Message,
                                                               ex.InnerException != null
                                                                   ? ex.InnerException.Message
                                                                   : string.Empty));
                return serviceResult;
            }
        }

        //Here is the sending logic so far
        private ServiceResult DoSend(Guid publishedTo, ServiceRequestMessage realMsg, object responseHandler = null)
        {
            ServiceResult result;
            var endPoint = GetEndPoint(publishedTo);

            if (endPoint == null)
            {
                result = new ServiceResult(false);
                result.ServerMessages.Add("There are not available endpoints");
            }
            else
            {
                result = DoSend(endPoint, realMsg);

                while (!result.Ok &&
                       ++PreselectedEndPoints[publishedTo] < Endpoints[publishedTo].Count)
                {
                    //PreselectedEndPoints[message.PublishedTo] += 1;
                    result = DoSend(GetEndPoint(publishedTo), realMsg);
                }

                if (!result.Ok)
                {
                    PreselectedEndPoints[publishedTo] = Endpoints[publishedTo].Count > 0 ? 0 : -1;
                }
                else
                {
                    if (responseHandler != null && result.AsyncResponseId != default(Guid))
                    {
                        //TODO: ALMACENAR ID PARA MANEJAR LA RESPUESTA ASYNCRONA, QUE SERA OTRA LLAMADA CON UN HANDLER QUE MANEJE LA RESPUESTA ASYNCRONA con timeout y cacheado em la mem cache
                    }
                }
            }
            return result;
        }


        //gets the first endpint for the message by priority, keeps track of the available and keeps checking failed with more priority

        private IEndPoint GetEndPoint(Guid destinationComponent)
        {
            if (destinationComponent == Guid.Empty)
                throw new ArgumentException("destinationComponent cannot be an empty value");

            if (!PreselectedEndPoints.ContainsKey(destinationComponent))
                lock (_preselectedEndPointLocker)
                    if (!PreselectedEndPoints.ContainsKey(destinationComponent))
                    {
                        if (!Endpoints.ContainsKey(destinationComponent))
                            lock (_endPointLocker)
                                if (!Endpoints.ContainsKey(destinationComponent))
                                    Endpoints.Add(destinationComponent,
                                                  ConnectivityManager.GetClientProxiesForComponent(destinationComponent));

                        PreselectedEndPoints[destinationComponent] = Endpoints[destinationComponent].Count > 0 ? 0 : -1;
                    }

            var idx = PreselectedEndPoints[destinationComponent];
            return idx >= 0 ? Endpoints[destinationComponent][idx] : null;
        }

        #endregion
    }
}