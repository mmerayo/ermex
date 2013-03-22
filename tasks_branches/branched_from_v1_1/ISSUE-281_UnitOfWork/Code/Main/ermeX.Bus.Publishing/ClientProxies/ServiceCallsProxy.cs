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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Castle.DynamicProxy;
using Common.Logging;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Component;
using ermeX.Domain.Services;
using ermeX.Entities.Entities;
using ermeX.Exceptions;
using ermeX.Transport.Interfaces;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Bus.Publishing.ClientProxies
{
    internal sealed class ServiceCallsProxy : IInterceptor, IServiceCallsProxy
    {
        [Inject]
        public ServiceCallsProxy(IServiceRequestManager serviceRequestsManager, 
			IDialogsManager dialogsManager,IBusSettings settings,ICanReadComponents componentReader,
			ICanReadServiceDetails serviceDetailsReader,
            IStatusManager statusManager)
        {
            if (dialogsManager == null) throw new ArgumentNullException("dialogsManager");
            if (settings == null) throw new ArgumentNullException("settings");
            if (statusManager == null) throw new ArgumentNullException("statusManager");

            if (serviceRequestsManager == null) throw new ArgumentNullException("serviceRequestsManager");
            ServiceRequestsManager = serviceRequestsManager;
            DialogsManager = dialogsManager;
            Settings = settings;
	        ComponentReader = componentReader;
	        ServiceDetailsReader = serviceDetailsReader;
	        StatusManager = statusManager;
        }

        private IServiceRequestManager ServiceRequestsManager { get; set; }
        private IDialogsManager DialogsManager { get; set; }
        private IBusSettings Settings { get; set; }
	    private ICanReadComponents ComponentReader { get; set; }
	    private ICanReadServiceDetails ServiceDetailsReader { get; set; }
	    private readonly ILog Logger=LogManager.GetLogger(StaticSettings.LoggerName);
        private IStatusManager StatusManager { get; set; }

        private Guid DestinationComponent { get; set; }

        #region IInterceptor Members

        public void Intercept(IInvocation invocation)
        {
            try
            {
                DoInvoke(invocation);
            }
            catch (ermeXComponentNotAvailableException ex)
            {
                Logger.Error(x=>x("{0}",ex));
                throw;
            }
            catch (ermeXUndefinedServiceException ex)
            {
                //Invoke service failed. Re-Exchanging definitions
                if(StatusManager.CurrentStatus==ComponentStatus.Running)
                    RefreshDefinition(invocation, ex.InterfaceName,ex.MethodName);
            }
        }


        private void RefreshDefinition(IInvocation invocation, string interfaceName, string methodName)
        {
            try
            {
                
                if (DestinationComponent.IsEmpty())
                {
                    DialogsManager.UpdateRemoteServiceDefinition(interfaceName, methodName);
                }
                else
                {
                    var appComponent = ComponentReader.Fetch(DestinationComponent);
                    if (appComponent == null)
                        throw new InvalidOperationException(string.Format("The component {0} could not be found",
                                                                          DestinationComponent));
                    //if(!appComponent.ExchangedDefinitions)
                    //    DialogsManager.EnsureDefinitionsAreExchanged(appComponent);
                    DialogsManager.UpdateRemoteServiceDefinition(interfaceName, methodName, appComponent);
                }

                DoInvoke(invocation);
            }
            catch (ermeXComponentNotAvailableException ex)
            {
                Logger.Error(x=>x( "{}", ex));
                throw;
            }
            catch (ermeXUndefinedServiceException exFatal)
            {
                Logger.Fatal(x=>x("Could not perform the invocation {0}.{1}",
                                           invocation.Method.DeclaringType.FullName, invocation.Method.Name),
                             exFatal);
                throw;
            }
        }

        //TODO huge refactor in the whole services invocation model pending
        private void DoInvoke(IInvocation invocation)
        {
            string interfaceTypeName = invocation.Method.DeclaringType.FullName;
            string methodName = invocation.Method.Name;

            //TODO: refactor
            ServiceDetails svc=null;
            IServiceOperationResult<object> result=null;
            if (DestinationComponent.IsEmpty()) //TODO: REFACTOR BOTH
            {
                var methods = ServiceDetailsReader.GetByMethodName(interfaceTypeName, methodName);
                if (methods.Count == 0)
                    throw new ermeXUndefinedServiceException(interfaceTypeName, methodName);
                if(methods.Count>1 && invocation.Method.ReturnType !=typeof(void))
                    throw new InvalidOperationException("There are several services that return values, the system only supports one service definition if returns values");

                foreach (var method in methods)
                {
                    svc = method;

                    //if (svc == null)
                    //    throw new ermeXUndefinedServiceException(interfaceTypeName, methodName);

                    Logger.Trace(x => x("Invoking service of component:{2} - {0}.{1}", svc.ServiceInterfaceTypeName,
                                        svc.ServiceImplementationMethodName, svc.Publisher));

                    //TODO MUST BE DONE IN PARALLEL as they dont return values
                    result = ServiceRequestsManager.DoRequest(invocation.Method.ReturnType,
                                                              new Tuple<Guid, Guid>(svc.Publisher,
                                                                                    svc.OperationIdentifier),
                                                              invocation.Arguments);
                   
                    if (result.OperationResult != OperationResultType.Success)
                        throw new ermeXServiceRequestReturnedErrors(interfaceTypeName, methodName, result.InnerException,
                                                           (DestinationComponent.IsEmpty()
                                                                ? (Guid?)null
                                                                : DestinationComponent));
                }

            }
            else
            {
                svc = ServiceDetailsReader.GetByMethodName(interfaceTypeName, methodName, DestinationComponent);
                if (svc == null)
                    throw new ermeXUndefinedServiceException(interfaceTypeName, methodName, DestinationComponent);

                Logger.Trace(x => x("Invoking service of component:{2} - {0}.{1}", svc.ServiceInterfaceTypeName,
                                    svc.ServiceImplementationMethodName, DestinationComponent));
                result =
                    ServiceRequestsManager.DoRequest(invocation.Method.ReturnType,
                                                     new Tuple<Guid, Guid>(DestinationComponent, svc.OperationIdentifier),
                                                     invocation.Arguments);


            }

            Logger.Debug(x => x("Invoked service of component:{2} - {0}.{1}.Result:{3}", svc.ServiceInterfaceTypeName,
                                svc.ServiceImplementationMethodName, svc.Publisher, result.OperationResult));

            if (result.OperationResult == OperationResultType.Success)
                invocation.ReturnValue = result.ResultValue;
            else
            {
                throw new ermeXServiceRequestReturnedErrors(interfaceTypeName, methodName, result.InnerException,
                                                            (DestinationComponent.IsEmpty()
                                                                 ? (Guid?) null
                                                                 : DestinationComponent));
            }

            Logger.Trace(x => x("Service: {0}.{1} Invoked SUCESSFULLY", svc.ServiceInterfaceTypeName,
                                svc.ServiceImplementationMethodName));
        }

        #endregion

        #region IServiceCallsProxy Members

        public void SetDestinationComponent(Guid componentId)
        {
            if (componentId.IsEmpty())
                throw new ArgumentException("componentId cannot be empty");
            DestinationComponent = componentId;
        }

        #endregion

        //TODO:async
        //public void Intercept(IInvocation invocation)
        //{
        //    string interfaceTypeName=invocation.Method.DeclaringType.FullName;
        //    string methodName = invocation.Method.Name;

        //    _returnType = invocation.Method.ReturnType;

        //    ServiceDetails svc= DataSource.GetByMethodName(interfaceTypeName, methodName);

        //    ServiceRequestsManager.DoRequest(svc.OperationIdentifier, _returnType, ResponseHandler, invocation.Arguments);
        //    _syncPoint.WaitOne(timeSpanWait);
        //    if (!_resultReceived)
        //        throw new TimeoutException("The service " + invocation.Method.DeclaringType.Name + "." + methodName +
        //                                   " didn't response in a timely manner. You can check the timeout settings.");

        //    invocation.ReturnValue = result;
        //}

        //private void ResponseHandler(ServiceOperationResult<object> result)
        //{
        //    this.result=result;

        //    //do staff
        //    _resultReceived = true;
        //    _syncPoint.Set();

        //}
    }
}