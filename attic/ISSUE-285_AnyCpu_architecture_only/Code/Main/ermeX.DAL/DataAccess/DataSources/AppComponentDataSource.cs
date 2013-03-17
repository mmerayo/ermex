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
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Ninject;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.ConfigurationManagement.Settings.Data;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.DataAccess.Providers;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.DataSources
{
    //TODO:refactor to base

    internal class AppComponentDataSource : DataSource<AppComponent>, IAppComponentDataSource, IDataAccessUsable<AppComponent>
    {
        private volatile IAutoRegistration _autoRegistration;
        private readonly object _syncLock=new object();

        //TODO: TEST SEVERAL COMPONENTS USING SAME DATABASE as there will be collisions
        [Inject]
        public AppComponentDataSource(IDalSettings dataAccessSettings, 
            IComponentSettings componentSettings,
            IConnectivityDetailsDataSource connectivityDetailsDataSource,
            IDataAccessExecutor dataAccessExecutor)
            : this(dataAccessSettings, componentSettings.ComponentId,dataAccessExecutor)
        {
            if (connectivityDetailsDataSource == null) throw new ArgumentNullException("connectivityDetailsDataSource");
            ConnectivityDetailsDataSource = connectivityDetailsDataSource;
            
        }

        public AppComponentDataSource(IDalSettings dataAccessSettings, Guid localComponentId, IDataAccessExecutor dataAccessExecutor)
            : base(dataAccessSettings, localComponentId,dataAccessExecutor)
        {
           
        }

         private IAutoRegistration AutoRegistration
         {
             get
             {
                 if (_autoRegistration == null)
                     lock (_syncLock)
                         if (_autoRegistration == null)
                             _autoRegistration = IoCManager.Kernel.Get<IAutoRegistration>();
                 return _autoRegistration;
             }
             
         }

        private IConnectivityDetailsDataSource ConnectivityDetailsDataSource { get; set; }

        protected override string TableName
        {
            get { return AppComponent.TableName; }
        }

        #region IAppComponentDataSource Members

        public AppComponent GetByComponentId(Guid componentId)
        {
            return base.GetItemByField("ComponentId", componentId);
        }

        public AppComponent GetByComponentId(ISession session, Guid componentId)
        {
            return base.GetItemByField(session,"ComponentId", componentId);
        }


        public int GetMaxLatency()
        {
            return base.GetMax("Latency");
        }

        public IList<AppComponent> GetOthers()
        {
            var result = DataAccessExecutor.Perform(session =>
                {
                    var resultData = GetItemsByFields(session, null,
                                                      new[]{new Tuple<string, object>("ComponentId", LocalComponentId)});

                    return new DataAccessOperationResult<IList<AppComponent>>()
                        {
                            Success = true,
                            ResultValue = resultData
                        };
                });
            if (!result.Success)
                throw new DataException("Couldnt perform the operation GetOthers");

            return result.ResultValue;


            //return GetAll().Where(x => x.ComponentId != LocalComponentId).ToList();
        }

        public bool SaveFromOtherComponent(AppComponent entity, Tuple<string, object>[] deterministicFilter, ConnectivityDetails connectivityDetails)
        {
            return AutoRegistration.CreateRemoteComponentInitialSetOfData(entity.ComponentId, connectivityDetails.Ip,
                                                          connectivityDetails.Port);
           
        }

        public IEnumerable<AppComponent> GetOtherComponentsWhereDefinitionsNotExchanged(bool running=false)
        {
            var result = DataAccessExecutor.Perform(session =>
                {
                    var resultData = GetItemsByFields(session,
                                                      new[]
                                                          {
                                                              new Tuple<string, object>("ExchangedDefinitions", false),
                                                              new Tuple<string, object>("IsRunning", running)
                                                          },
                                                      new[] {new Tuple<string, object>("ComponentId", LocalComponentId)});


                return new DataAccessOperationResult<IList<AppComponent>>()
                {
                    Success = true,
                    ResultValue = resultData
                };
            });
            if (!result.Success)
                throw new DataException("Couldnt perform the operation GetOtherComponentsWhereDefinitionsNotExchanged");

            return result.ResultValue;

        }

        #endregion

        protected override bool BeforeUpdating(AppComponent entity, ISession session)
        {
            var existing = GetByComponentId(session, entity.ComponentId);
            session.Evict(existing);
            if (existing == null)
                return true;
            if (existing.Version > entity.Version)
                return false;

            return true;
        }

        public override void Save(AppComponent entity)
        {
            if (entity.ComponentExchanges != null && entity.ComponentExchanges != entity.ComponentId && entity.ComponentExchanges != entity.ComponentOwner)
            {
                Debugger.Launch();
                throw new InvalidOperationException(
                   "The component that is in charge of exchanging the data must be one of the involved in the data exchanging");
            }

            base.Save(entity);
        }

      


        protected override void CleanExternalData(AppComponent externalEntity)
        {
            externalEntity.ComponentExchanges = null;
        }

        protected override void UpdateWhenExternal(AppComponent entity, AppComponent existingEntity)
        {
            entity.ComponentExchanges = existingEntity.ComponentExchanges;
            entity.ExchangedDefinitions = existingEntity.ExchangedDefinitions;
            entity.IsRunning = existingEntity.IsRunning;
            base.UpdateWhenExternal(entity, existingEntity);
        }

        public void SetComponentRunningStatus(Guid componentId, ComponentStatus newStatus, bool exchangedDefinitions = false)
        {
            var result = DataAccessExecutor.Perform(session=>
                {
                    var localComponent = GetByComponentId(session,componentId);
                    localComponent.IsRunning = newStatus == ComponentStatus.Running;
                    localComponent.ExchangedDefinitions = exchangedDefinitions;

                    Save(session, localComponent);
                    return new DataAccessOperationResult<bool>()
                    {
                        Success = true,
                    };
                });
            if (!result.Success)
                throw new DataException("Couldnt perform the operation SetComponentRunningStatus");

        }

        public void UpdateRemoteComponentLatency(Guid remoteComponentId, int currentRequestMilliseconds)
        {
            var result = DataAccessExecutor.Perform(session =>
                {
                    var senderComponent = GetByComponentId(session, remoteComponentId);
                    if (senderComponent != null)
                    {
                        senderComponent.Latency = (senderComponent.Latency + currentRequestMilliseconds)/2;
                        Save(session, senderComponent);
                    }
                    return new DataAccessOperationResult<bool>()
                        {
                            Success = true,
                        };
                });
            if (!result.Success)
                throw new DataException("Couldnt perform the operation UpdateLatency");

        }
    }
}