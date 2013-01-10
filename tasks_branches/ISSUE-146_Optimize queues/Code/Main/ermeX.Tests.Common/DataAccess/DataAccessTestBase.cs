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
using NUnit.Framework;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.Entities.Entities;

namespace ermeX.Tests.Common.DataAccess
{
    [TestFixture]
    abstract class DataAccessTestBase
    {
        #region infrastructure

        protected readonly Guid LocalComponentId = Guid.NewGuid();
        protected readonly Guid RemoteComponentId = Guid.NewGuid();

        #region Datahelper

        protected const string SchemaName = "[ClientComponent]";
        private readonly object _syncLock = new object();
        private readonly Dictionary<DbEngineType, DataAccessTestHelper> _dataHelpers = new Dictionary<DbEngineType, DataAccessTestHelper>(Enum.GetValues(typeof(DbEngineType)).Length);

        protected DataAccessTestHelper GetDataHelper(DbEngineType engineType)
        {
            if (!_dataHelpers.ContainsKey(engineType))
                lock (_syncLock)
                    if (!_dataHelpers.ContainsKey(engineType))
                        _dataHelpers.Add(engineType, new DataAccessTestHelper(engineType,CreateDatabase, SchemaName, LocalComponentId, RemoteComponentId));

            return _dataHelpers[engineType];
        }
        #endregion

        #endregion

        private bool _createDatabase=true;

        protected virtual List<DataSchemaType> SchemasToApply
        {
            get
            {
                return new List<DataSchemaType>
                           {
                               DataSchemaType.ClientComponent
                           };
            }
        }

        /// <summary>
        /// specifies if the test db provider creates a new database
        /// </summary>
        protected bool CreateDatabase
        {
            get { return _createDatabase; }
            set { _createDatabase = value; }
        }

        [TestFixtureTearDown]
        public virtual void OnFixtureTearDown()
        {
            foreach (var value in _dataHelpers.Values)
            {
                value.Dispose();
            }
        }

        [TearDown]
        public virtual void OnTearDown()
        {
            foreach (var value in _dataHelpers.Values)
            {
                value.ClearData();
            }
        }

        [SetUp]
        public virtual void OnStartUp()
        {
            
        }

        [TestFixtureSetUp]
        public virtual void OnFixtureSetup()
        {
        }

        private readonly Dictionary<DbEngineType, DataAccessExecutor> _dataAccessExecutors = new Dictionary<DbEngineType, DataAccessExecutor>();
        protected DataAccessExecutor GetdataAccessExecutor(DbEngineType engineType)
        {
            if (!_dataAccessExecutors.ContainsKey(engineType))
            {
                var dataAccessExecutor = new DataAccessExecutor(GetDataHelper(engineType).DataAccessSettings);
                _dataAccessExecutors.Add(engineType, dataAccessExecutor);
            }
            return _dataAccessExecutors[engineType];
        }

        //TODO: MOVE THE FOLLOWING METHODS TO A GENERIC PROVIDER

        #region Data sources

        private readonly Dictionary<DbEngineType, BusMessageDataSource> _busMessageDataSources = new Dictionary<DbEngineType, BusMessageDataSource>();
        protected BusMessageDataSource GetBusMessageDataSource(DbEngineType engineType)
        {
            if (!_busMessageDataSources.ContainsKey(engineType))
            {
                var dataAccessExecutor = GetdataAccessExecutor(engineType);
                var busMessageDataSource = new BusMessageDataSource(dataAccessExecutor.DalSettings, LocalComponentId,
                                                                    dataAccessExecutor);
                _busMessageDataSources.Add(engineType, busMessageDataSource);
            }
            return _busMessageDataSources[engineType];
        }

        private readonly Dictionary<DbEngineType, OutgoingMessagesDataSource> _outgoingMessageDataSources = new Dictionary<DbEngineType, OutgoingMessagesDataSource>();
        protected OutgoingMessagesDataSource GetOutgoingMessageDataSource(DbEngineType engineType)
        {
            if (!_outgoingMessageDataSources.ContainsKey(engineType))
            {
                var dataAccessExecutor = GetdataAccessExecutor(engineType);
                var dataSource = new OutgoingMessagesDataSource(dataAccessExecutor.DalSettings, LocalComponentId,
                                                                    dataAccessExecutor);
                _outgoingMessageDataSources.Add(engineType, dataSource);
            }
            return _outgoingMessageDataSources[engineType];
        }

        private readonly Dictionary<DbEngineType, ChunkedServiceRequestMessageDataSource> _chunkDataSources = new Dictionary<DbEngineType, ChunkedServiceRequestMessageDataSource>();
        protected ChunkedServiceRequestMessageDataSource GetChunkedServiceRequestMessageDataSource(DbEngineType engineType)
        {
            if (!_chunkDataSources.ContainsKey(engineType))
            {
                var dataAccessExecutor = GetdataAccessExecutor(engineType);
                var ds = new ChunkedServiceRequestMessageDataSource(dataAccessExecutor.DalSettings, LocalComponentId,
                                                                    dataAccessExecutor);
                _chunkDataSources.Add(engineType, ds);
            }
            return _chunkDataSources[engineType];
        } 

        #endregion

        #region AllRecords

        protected IList<OutgoingMessage> GetOutgoingMessages(DbEngineType engineType)
        {
            return GetOutgoingMessageDataSource(engineType).GetAll();
        }

        protected BusMessageData GetBusMessage(DbEngineType engineType, int id)
        {
            return GetBusMessageDataSource(engineType).GetById(id);
        }

        #endregion
    }
}
