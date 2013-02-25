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
using System.Data;
using System.Diagnostics;
using NHibernate;
using NHibernate.Criterion;
using Ninject;
using ermeX;
using ermeX.Common;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.DataSources
{
    sealed class AutoRegistration:IAutoRegistration
    {
        private IBusSettings BusSettings { get; set; }
        private IAppComponentDataSource AppComponentDataSource { get; set; }
        private IServiceDetailsDataSource ServiceDetailsDataSource { get; set; }
        private IConnectivityDetailsDataSource ConnectivityDetailsDataSource { get; set; }
        private IStatusManager StatusManager { get; set; }

        [Inject]
        public AutoRegistration(IBusSettings busSettings, IAppComponentDataSource appComponentDataSource,
                              IServiceDetailsDataSource serviceDetailsDataSource,
                              IConnectivityDetailsDataSource connectivityDetailsDataSource,
            IStatusManager statusManager, IDalSettings dataAccessSettings
            ,IDataAccessExecutor dataAccessExecutor
            )
        {
            if (busSettings == null) throw new ArgumentNullException("busSettings");
            if (appComponentDataSource == null) throw new ArgumentNullException("appComponentDataSource");
            if (serviceDetailsDataSource == null) throw new ArgumentNullException("serviceDetailsDataSource");
            if (connectivityDetailsDataSource == null) throw new ArgumentNullException("connectivityDetailsDataSource");
            if (statusManager == null) throw new ArgumentNullException("statusManager");
            if (dataAccessSettings == null) throw new ArgumentNullException("dataAccessSettings");
            if (dataAccessExecutor == null) throw new ArgumentNullException("dataAccessExecutor");
            BusSettings = busSettings;
            AppComponentDataSource = appComponentDataSource;
            ServiceDetailsDataSource = serviceDetailsDataSource;
            ConnectivityDetailsDataSource = connectivityDetailsDataSource;
            StatusManager = statusManager;
            DataAccessSettings = dataAccessSettings;
            DataAccessExecutor = dataAccessExecutor;
        }

        private IDalSettings DataAccessSettings { get; set; }
        private IDataAccessExecutor DataAccessExecutor { get; set; }


        public bool CreateRemoteComponentInitialSetOfData(Guid remoteComponentId, string ip, int port)
        {
          
            var result = DataAccessExecutor.Perform(session =>
                {
                    bool isNew= AddComponentFromRemote(remoteComponentId, session).ResultValue;
                    AddConnectivityDetailsFromRemote(remoteComponentId, ip, port, session);
                    RegisterSystemServices(session, remoteComponentId);
                    return new DataAccessOperationResult<bool>()
                        {
                            Success = true,
                            ResultValue=isNew
                        };
                });
            if (!result.Success)
                throw new DataException("Couldnt perform the operation CreateRemoteComponentInitialSetOfData");
            return result.ResultValue;

        }

        private DataAccessOperationResult<bool> AddConnectivityDetailsFromRemote(Guid remoteComponentId, string ip, int port, ISession session)
        {
            var dataAccessUsableConnectivityDetails = ((IDataAccessUsableConnectivityDetails) ConnectivityDetailsDataSource);
            ConnectivityDetails connectivityDetails =
                dataAccessUsableConnectivityDetails.GetByComponentId(session,remoteComponentId) ??
                new ConnectivityDetails();
            connectivityDetails.ComponentOwner = BusSettings.ComponentId;
            connectivityDetails.Ip = ip;
            connectivityDetails.Port = port;
            connectivityDetails.ServerId = remoteComponentId;
            connectivityDetails.Version = DateTime.MinValue.Ticks + 1;


            dataAccessUsableConnectivityDetails.Save(session, connectivityDetails);
            return new DataAccessOperationResult<bool>()
            {
                Success = true
                
            };
        }

        private DataAccessOperationResult<bool> AddComponentFromRemote(Guid remoteComponentId, ISession session)
        {
            AppComponent appComponent =
                ((IDataAccessUsable<AppComponent>) AppComponentDataSource).GetItemByField(session, "ComponentId",
                                                                                          remoteComponentId);
            bool isNew = false;
            if (appComponent == null)
            {
                appComponent = new AppComponent
                    {
                        ComponentOwner = BusSettings.ComponentId,
                        ComponentId = remoteComponentId,
                        IsRunning = false,
                    };
                isNew = true;
            }

            appComponent.ExchangedDefinitions = false;
            ((IDataAccessUsable<AppComponent>) AppComponentDataSource).Save(session, appComponent);
            return new DataAccessOperationResult<bool>()
            {
                Success = true,
                ResultValue = isNew
            };
        }

        private DataAccessOperationResult<object> RegisterSystemServices(ISession session, Guid componentId )
        {
            RegisterSystemServices(session, TypesHelper.GetTypeFromDomainByClassName("IHandshakeService"), componentId);
            RegisterSystemServices(session, TypesHelper.GetTypeFromDomainByClassName("IMessageSuscriptionsService"), componentId);
            RegisterSystemServices(session, TypesHelper.GetTypeFromDomainByClassName("IPublishedServicesDefinitionsService"), componentId);
            return new DataAccessOperationResult<object>()
            {
                Success = true
            };
        }


        private void RegisterSystemServices(ISession session, Type typeService, Guid remoteComponentId)
        {
            if (typeService == null) throw new ArgumentNullException("typeService");
            IEnumerable<string> serviceOperationAttributes = ServiceOperationAttribute.GetServiceNames(typeService);
            foreach (var name in serviceOperationAttributes)
                RegisterSystemService (session, remoteComponentId, name, typeService);
        }

        private void RegisterSystemService(ISession session, Guid componentId, string serviceImplementationMethodName, Type serviceInterfaceType)
        {
            if (serviceInterfaceType == null) throw new ArgumentNullException("serviceInterfaceType");
            var isLocal = BusSettings.ComponentId == componentId;
            var serviceDetails = new ServiceDetails
            //TODO: Issue 22, this is repeated in other points locate them and refactor
            {
                ComponentOwner = BusSettings.ComponentId,
                OperationIdentifier =
                    ServiceOperationAttribute.GetOperationIdentifier(
                        serviceInterfaceType,
                        serviceImplementationMethodName),
                //TODO: get FROM BASE CLASS
                Publisher = componentId,
                ServiceImplementationMethodName = serviceImplementationMethodName,
                ServiceImplementationTypeName = isLocal? IoCManager.Kernel.Get(serviceInterfaceType).GetType().FullName :"<<REMOTE>>",
                ServiceInterfaceTypeName = serviceInterfaceType.FullName,
                IsSystemService = true,
                Version = DateTime.MinValue.Ticks + 1 //to force to update from node
            };

            ((IDataAccessUsable<ServiceDetails>)ServiceDetailsDataSource).Save(session, serviceDetails);
        }

        /// <summary>
        ///   creates the initial set of data
        /// </summary>
        public void CreateLocalSetOfData(int port)
        {
            var result = DataAccessExecutor.Perform(session =>
            {
                //component
                var appComponent = new AppComponent
                {
                    ComponentOwner = BusSettings.ComponentId,
                    ComponentId = BusSettings.ComponentId,
                    IsRunning = StatusManager.CurrentStatus == ComponentStatus.Running,
                    ExchangedDefinitions = true
                };
                ((IDataAccessUsable<AppComponent>)AppComponentDataSource).Save(session, appComponent);

                //connectivitydetails
                var connectivityDetails = new ConnectivityDetails
                {
                    ComponentOwner = BusSettings.ComponentId,
                    ServerId = BusSettings.ComponentId,
                    Ip = Networking.GetLocalhostIp(),
                    Port = port
                };
                ((IDataAccessUsable<ConnectivityDetails>)ConnectivityDetailsDataSource).Save(session, connectivityDetails);

                RegisterSystemServices(session, BusSettings.ComponentId);

                return new DataAccessOperationResult<int>()
                {
                    Success = true,
                };
            });
            if (!result.Success)
                throw new DataException("Couldnt perform the operation GetAll");
        }
    }
}
