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
    }
}
