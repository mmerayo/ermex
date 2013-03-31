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
using System.Linq;
using NHibernate;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.ConfigurationManagement.Settings.Data;
using ermeX.DAL.DataAccess.Providers;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.DataSources
{
    internal class ServiceDetailsDataSource : DataSource<ServiceDetails>, IServiceDetailsDataSource, IDataAccessUsable<ServiceDetails>
    {
        private const string RemoteTypeImplementorValue = "<<REMOTE>>";

        [Inject]
        public ServiceDetailsDataSource(IDalSettings settings, IComponentSettings componentSettings, IDataAccessExecutor dataAccessExecutor)
            : base(settings, componentSettings.ComponentId,dataAccessExecutor)
        {
        }

        public ServiceDetailsDataSource(IDalSettings settings, Guid componentId, IDataAccessExecutor dataAccessExecutor)
            : base(settings, componentId,dataAccessExecutor)
        {
        }

        protected override string TableName
        {
            get { return ServiceDetails.TableName; }
        }

        #region IServiceDetailsDataSource Members

        /// <summary>
        ///   Gets the local server service details
        /// </summary>
        /// <param name="operationId"> </param>
        /// <returns> </returns>
        public ServiceDetails GetByOperationId(Guid operationId)
        {
            return GetByOperationId(LocalComponentId, operationId);
        }


        /// <summary>
        ///   Gets any server service details
        /// </summary>
        /// <param name="operationId"> </param>
        /// <returns> </returns>
        public ServiceDetails GetByOperationId(Guid publisher, Guid operationId)
        {
            if (publisher.IsEmpty())
                return base.GetItemByField("OperationIdentifier", operationId);

            return
                base.GetItemsByField("OperationIdentifier", operationId).SingleOrDefault(x => x.Publisher == publisher);
        }

        //public override void Save(ServiceDetails entity)
        //{
        //    //TODO SOLVE THIS BOTCH
        //    if (entity == null) throw new ArgumentNullException("entity");
        //    lock (this)
        //    {
        //        var itemsByField = GetItemsByField("ServiceInterfaceTypeName", entity.ServiceInterfaceTypeName)
        //            .SingleOrDefault(x => x.OperationIdentifier != entity.OperationIdentifier
        //                                  && x.ServiceImplementationMethodName == entity.ServiceImplementationMethodName
        //                                  && x.ComponentOwner == entity.ComponentOwner
        //                                  && x.Publisher == entity.Publisher
        //            );
        //        if (itemsByField != null)
        //            throw new InvalidOperationException(
        //                "The combination of service interface type,method name, publisher and owner must be unique");
        //        base.Save(entity);
        //    }
        //}

        protected override void CleanExternalData(ServiceDetails externalEntity)
        {
            externalEntity.ServiceImplementationTypeName = RemoteTypeImplementorValue;
        }

        public IList<ServiceDetails> GetByInterfaceType(Type interfaceType)
        {
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");
            return GetByInterfaceType(interfaceType.FullName);
        }

        public IList<ServiceDetails> GetByInterfaceType(string interfaceTypeFullName)
        {
            if (string.IsNullOrEmpty(interfaceTypeFullName)) throw new ArgumentNullException("interfaceTypeFullName");
            return GetItemsByField("ServiceInterfaceTypeName", interfaceTypeFullName);
        }


        public IList<ServiceDetails> GetByMethodName(string interfaceTypeName, string methodName)
        {
            return GetItemsByFields(new[]
                                       {
                                           new Tuple<string, object>("ComponentOwner", LocalComponentId),//TODO:redundant
                                           new Tuple<string, object>("ServiceInterfaceTypeName", interfaceTypeName),
                                           new Tuple<string, object>("ServiceImplementationMethodName", methodName)
                                       });
        }
        public IList<ServiceDetails> GetByMethodName(ISession session, string interfaceTypeName, string methodName)
        {
            return GetItemsByFields(session,new[]
                                       {
                                           new Tuple<string, object>("ComponentOwner", LocalComponentId),//TODO:redundant
                                           new Tuple<string, object>("ServiceInterfaceTypeName", interfaceTypeName),
                                           new Tuple<string, object>("ServiceImplementationMethodName", methodName)
                                       });
        }

        public ServiceDetails GetByMethodName(ISession session, string interfaceTypeName, string methodName, Guid publisherComponent)
        {
            return GetItemByFields(session,
                new[]
                    {
                        new Tuple<string, object>("ComponentOwner", LocalComponentId),//TODO:redundant
                        new Tuple<string, object>("ServiceInterfaceTypeName", interfaceTypeName),
                        new Tuple<string, object>("ServiceImplementationMethodName", methodName),
                        new Tuple<string, object>("Publisher", publisherComponent)
                    });
        }
        //TODO: FOR EVERY DATASOURCE: ensure that in a shared db only access to its own items
        public ServiceDetails GetByMethodName(string interfaceTypeName, string methodName, Guid publisherComponent)
        {
            return GetItemByFields(
                new[]
                    {
                        new Tuple<string, object>("ComponentOwner", LocalComponentId), //TODO:redundant
                        new Tuple<string, object>("ServiceInterfaceTypeName", interfaceTypeName),
                        new Tuple<string, object>("ServiceImplementationMethodName", methodName),
                        new Tuple<string, object>("Publisher", publisherComponent)
                    });
        }

        public IList<ServiceDetails> GetLocalCustomServices()
        {

                 var result = DataAccessExecutor.Perform(session =>
                     {
                         var resultData = GetItemsByFields(session,
                                                           new Tuple<string, object>("Publisher", LocalComponentId),
                                                           new Tuple<string, object>("IsSystemService", false)
                             );


                return new DataAccessOperationResult<IList<ServiceDetails>>()
                {
                    Success = true,
                    ResultValue = resultData
                };
            });
            if (!result.Success)
                throw new DataException("Couldnt perform the operation GetLocalCustomServices");

            return result.ResultValue;

        }

        #endregion

        protected override bool BeforeUpdating(ServiceDetails entity, ISession session)
        {
            var existing = GetByMethodName(session, entity.ServiceInterfaceTypeName, entity.ServiceImplementationMethodName,
                                           entity.Publisher);
            session.Evict(existing);
            if (existing == null)
                return true;

            if (existing.Version > entity.Version)
                return false;
            return true;
        }
    }
}