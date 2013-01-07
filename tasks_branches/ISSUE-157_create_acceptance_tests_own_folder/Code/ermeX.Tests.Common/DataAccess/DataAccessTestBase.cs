// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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
