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
using NHibernate;
using ermeX.Entities.Base;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces
{
    /// <summary>
    /// Allow to exchange sessions between datasources
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <remarks>its meant for internal use only</remarks>
    internal interface IDataAccessUsable<TEntity> where TEntity : ModelBase
    {
        DataAccessOperationResult<TEntity> GetById(ISession session, int id);
        DataAccessOperationResult<IList<TEntity>> GetAll(ISession session, params Tuple<string, bool>[] sortByParams);
        int CountItems(ISession session,string propertyName, object propertyValue);
        int CountItems(ISession session,Tuple<string, object>[] arguments );
        DataAccessOperationResult<bool> Save(ISession session, TEntity entity);
        DataAccessOperationResult<bool> Save(ISession session, IEnumerable<TEntity> entities);

        TEntity GetItemByField<TFieldType>(ISession session, string propertyName, TFieldType propertyValue);
        TEntity GetItemByFields(ISession session, Tuple<string, object>[] arguments);
        IList<TEntity> GetItemsByFields(ISession session, params Tuple<string, object>[] arguments);
        IList<TEntity> GetItemsByField<TFieldType>(ISession session, string propertyName, TFieldType propertyValue);

        DataAccessOperationResult<bool> SaveFromOtherComponent(ISession session, TEntity entity,
                                                               Tuple<string, object>[] deterministicFilter);

        DataAccessOperationResult<bool> RemoveAll(ISession session);
    }

    internal interface IDataAccessUsableConnectivityDetails:IDataAccessUsable<ConnectivityDetails>
    {
        ConnectivityDetails GetByComponentId(ISession session,Guid componentId);
    }
}