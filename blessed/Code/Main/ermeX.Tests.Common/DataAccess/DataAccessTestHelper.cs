// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.Entities.Base;
using ermeX.Entities.Entities;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Versioning;

namespace ermeX.Tests.Common.DataAccess
{
    internal sealed class DataAccessTestHelper:IDisposable
    {
        private static List<DataSchemaType> _result;

        private readonly object _syncLock=new object();  

       
        private bool _dbCreated;

        public DataAccessTestHelper(DbEngineType engineType, bool createDb, string schemaName)
            : this(engineType,createDb, schemaName, null, null, null)
        {
        }
        public DataAccessTestHelper(DbEngineType engineType, bool createDb, string schemaName, Guid? localComponentId, Guid? remoteComponentId)
            : this(engineType,createDb, schemaName, localComponentId, remoteComponentId, null)
        {
        }

        public DataAccessTestHelper(DbEngineType engineType, bool createDb, string schemaName, Guid? localComponentId, Guid? remoteComponentId, Action<DbEngineType> setConfigurationMethod)
        {
            EngineType = engineType;
            SchemaName = schemaName;
            if(localComponentId.HasValue)
                LocalComponentId = localComponentId.Value;
            if(remoteComponentId.HasValue)
                RemoteComponentId = remoteComponentId.Value;

            //fixturesetup
            RemoveDatabase = true;
            if(setConfigurationMethod!=null)
                setConfigurationMethod(engineType);
            //testsetup
            if(createDb)
                CreateDb();

        }

        public void Dispose()
        {
            //teardown
            ClearData();

            //fixtureteardown
            if (RemoveDatabase)
            {
                QueryTestHelper.DeleteDbDefinitions();
                TestSettingsProvider.DropDatabase(EngineType); 
            }
        }

        public void ClearData()
        {
            QueryTestHelper.RemoveAllData();
        }

        public List<DataSchemaType> SchemasToApply
        {
            get
            {
                if (_result == null)
                    lock (_syncLock)
                        if (_result == null)
                            _result = new List<DataSchemaType>(){DataSchemaType.ClientComponent};

                return _result;
            }
        }

        public DbEngineType EngineType { get; private set; }
        public string SchemaName { get; private set; }
        public Guid LocalComponentId { get; private set; }
        public Guid RemoteComponentId { get; private set; }


        private void CreateDb()
        {
            if (!_dbCreated)
            {

                QueryTestHelper.DeleteDbDefinitions();//TODO: THIS SMELLS HERE

                var target = new VersionUpgradeHelper();
                IDalSettings settingsSource = TestSettingsProvider.GetDataAccessSettingsSource(EngineType,
                                                                                               SchemasToApply);
                target.RunDataSchemaUpgrades(settingsSource.SchemasApplied,
                                             settingsSource.ConfigurationConnectionString,
                                             settingsSource.ConfigurationSourceType);
                _dbCreated = true;

            }
        }

        private volatile QueryHelper _queryTestHelper = null;
        public QueryHelper QueryTestHelper
        {
            get
            {
                if (_queryTestHelper == null)
                    lock (_syncLock) if (_queryTestHelper == null)
                       _queryTestHelper= QueryHelper.GetHelper(EngineType, DataAccessSettings.ConfigurationConnectionString);
                return _queryTestHelper;
            }
        }

        /// <summary>
        /// Indicates if the db will be removed after the test
        /// </summary>
        public bool RemoveDatabase { get; set; }

        private readonly Dictionary<Type,string> _tableNames=new Dictionary<Type, string>(); 
        public List<TResult> GetList<TResult>() where TResult : ModelBase, new()
        {
            if (!_tableNames.ContainsKey(typeof (TResult)))
                lock (_syncLock)
                    if (!_tableNames.ContainsKey(typeof(TResult)))
                    {
                        FieldInfo[] fieldInfos =
                            typeof (TResult).GetFields(BindingFlags.Static | BindingFlags.NonPublic |
                                                       BindingFlags.FlattenHierarchy);

                        var theConstant = fieldInfos.SingleOrDefault(x => x.IsLiteral && x.Name == "TableName");

                        _tableNames.Add(typeof(TResult), (string) theConstant.GetValue(null));
                    }
            
            DataTable tableFromDb = GetTableFromDb(_tableNames[typeof(TResult)]);
            return QueryHelper.ObjectsFromTable<TResult>(tableFromDb);
        }

        public IDalSettings DataAccessSettings
        {
            get { return TestSettingsProvider.GetDataAccessSettingsSource(EngineType, SchemasToApply); }
        }


        public string GetIdFieldName(string tableName)
        {
            return string.Format("{0}_Id", tableName);
        }

        public string LastIdSqlQuery(string tableName)
        {
            string idFieldName = GetIdFieldName(tableName);
            return QueryTestHelper.GetLastIdQuery(SchemaName, tableName, idFieldName);
        }

        public string InsertAppComponentQuery( Guid componentId, Guid ownerComponentId,
                                                 int latency, DateTime versionUtc, bool isRunning,
                                                 bool exchangedDefinitions)
        {
            return
                string.Format(
                    "Insert into {2}.{0} ({0}_ComponentId,{0}_ComponentOwner,{0}_Latency,{0}_Version,{0}_IsRunning,{0}_ExchangedDefinitions) Values ('{1}','{3}',{4},{5},'{6}','{7}')",
                    AppComponent.TableName, componentId, SchemaName, ownerComponentId, latency, versionUtc.Ticks,
                    isRunning, exchangedDefinitions);
        }

        public int InsertAppComponent(Guid componentId, Guid ownerComponentId, int latency,
                                         bool isRunning, bool exchangedDefinitions)
        {
            QueryTestHelper.ExecuteNonQuery(InsertAppComponentQuery(componentId, ownerComponentId,
                                                                            latency, DateTime.UtcNow, isRunning,
                                                                            exchangedDefinitions));
            var id =  QueryTestHelper.ExecuteScalar<int>(LastIdSqlQuery("Components"));

            return id;
        }

        public int InsertAppComponent( Guid componentId, Guid ownerComponentId,
                                         DateTime versionUtc, int latency, bool isRunning, bool exchangedDefinitions)
        {
            QueryTestHelper.ExecuteNonQuery(InsertAppComponentQuery(componentId, ownerComponentId,
                                                                            latency, versionUtc, isRunning,
                                                                            exchangedDefinitions));
            string lastIdSqlQuery = LastIdSqlQuery("Components");
            var id =  QueryTestHelper.ExecuteScalar<int>(lastIdSqlQuery);


            return id;
        }

        public AppComponent GetNewAppComponent()
        {
            int idComponent = InsertAppComponent(Guid.NewGuid(), LocalComponentId, 0, false, false);

            IDalSettings dataAccessSettingsSource = TestSettingsProvider.GetDataAccessSettingsSource(EngineType, SchemasToApply);
            var dataAccessExecutor = new DataAccessExecutor(dataAccessSettingsSource);
            var dsComponent =
                new AppComponentDataSource(dataAccessSettingsSource,
                                           LocalComponentId,dataAccessExecutor);

            return dsComponent.GetById(idComponent);
        }

        public AppComponent GetNewAppComponent(Guid componentId, Guid ownerComponentId)
        {
            int idComponent = InsertAppComponent(componentId, ownerComponentId, 0, false, false);

            IDalSettings dataAccessSettingsSource = TestSettingsProvider.GetDataAccessSettingsSource(EngineType, SchemasToApply);
            var dataAccessExecutor = new DataAccessExecutor(dataAccessSettingsSource);
            var dsComponent =
                new AppComponentDataSource(dataAccessSettingsSource,
                                           ownerComponentId,dataAccessExecutor);

            return dsComponent.GetById(idComponent);
        }

        public int InsertOutgoingMessageSuscriptions(string messageType, DateTime updateTime,
                                                        Guid componentId, Guid componentOwnerId)
        {
            string sqlQuery = String.Format(
                "INSERT INTO ClientComponent.{4} ([{4}_BizMessageFullTypeName],[{4}_DateLastUpdateUtc],[{4}_ComponentId],{4}_ComponentOwner,{4}_Version) VALUES('{0}',{1},'{2}','{3}',{5})",
                messageType, updateTime.Ticks, componentId, componentOwnerId, OutgoingMessageSuscription.TableName,
                DateTime.UtcNow.Ticks);
            QueryTestHelper.ExecuteNonQuery(
                sqlQuery);
            var id = QueryTestHelper.ExecuteScalar<int>(LastIdSqlQuery(OutgoingMessageSuscription.TableName));


            return id;
        }

        public int InsertIncomingMessageSuscriptions(string messageType, DateTime updateTime,
                                                        Guid ownerComponentId, Guid suscriptionHandlerId,
                                                        string handlerType)
        {
            string sqlQuery = String.Format(
                "INSERT INTO ClientComponent.{3} ([{3}_BizMessageFullTypeName],[{3}_DateLastUpdateUtc],{3}_ComponentOwner,{3}_SuscriptionHandlerId,{3}_Version,{3}_HandlerType) VALUES('{0}',{1},'{2}','{4}',{5},'{6}')",
                messageType, updateTime.Ticks, ownerComponentId, IncomingMessageSuscription.TableName,
                suscriptionHandlerId, DateTime.UtcNow.Ticks, handlerType);
            QueryTestHelper.ExecuteNonQuery(
                sqlQuery);
            var id = QueryTestHelper.ExecuteScalar<int>(LastIdSqlQuery(IncomingMessageSuscription.TableName));


            return id;
        }

        public int InsertOutgoingMessage( Guid destination, Guid owner,
                                            int busMessageId, DateTime timePublished, int tries,
                                            bool errored)
        {
            string sqlQuery = String.Format(
                "insert into ClientComponent.{6} ([{6}_PublishedBy],[{6}_PublishedTo],[{6}_BusMessageId],[{6}_TimePublishedUtc],[{6}_Tries],[{6}_Failed],{6}_ComponentOwner,{6}_Version,{6}_Delivering) VALUES('{0}','{1}','{2}',{3},'{4}','{5}','{0}',{7},0)",
                owner, destination, busMessageId,  timePublished.Ticks, tries, errored,
                OutgoingMessage.FinalTableName, DateTime.UtcNow.Ticks);
            QueryTestHelper.ExecuteNonQuery(
                sqlQuery);
            string lastIdSqlQuery = LastIdSqlQuery(OutgoingMessage.FinalTableName);
            var id = QueryTestHelper.ExecuteScalar<int>(lastIdSqlQuery);


            return id;
        }

        public int InsertIncomingMessage( Guid destination, Guid owner,
                                            int busMessageId, DateTime timePublished,
                                            DateTime timeReceived, Guid suscriptionHandler)
        {
            string sqlQuery = String.Format(
                "insert into ClientComponent.{5} ([{5}_PublishedBy],[{5}_PublishedTo],[{5}_BusMessageId],[{5}_TimePublishedUtc],{5}_ComponentOwner,{5}_TimeReceivedUtc,{5}_SuscriptionHandlerId,{5}_Version) VALUES('{0}','{1}','{2}',{3},'{0}',{4},'{6}',{7})",
                owner, destination, busMessageId,  timePublished.Ticks, timeReceived.Ticks,
                IncomingMessage.FinalTableName, suscriptionHandler, DateTime.UtcNow.Ticks);
            QueryTestHelper.ExecuteNonQuery(
                sqlQuery);
            string lastIdSqlQuery = LastIdSqlQuery(IncomingMessage.FinalTableName);
            var id = QueryTestHelper.ExecuteScalar<int>(lastIdSqlQuery);


            return id;
        }

        public int InsertBusMessage(Guid componentId, Guid ownerComponentId, DateTime versionUtc, Guid messageId, Guid publisher, DateTime createdTimeUtc, string jsonMessage, int status)
        {
            string sqlQuery = String.Format(
                "INSERT INTO [ClientComponent].[{0}]" +
                "([{0}_MessageId],[{0}_CreatedTimeUtc],[{0}_Publisher],[{0}_JsonMessage],[{0}_Version],[{0}_ComponentOwner],[{0}_Status])" +
                "VALUES ('{1}',{2},'{3}','{4}',{5},'{6}',{7})", BusMessageData.TableName, messageId, createdTimeUtc.Ticks, publisher, jsonMessage, versionUtc.Ticks,
                componentId,status);
            QueryTestHelper.ExecuteNonQuery(
                sqlQuery);
            string lastIdSqlQuery = LastIdSqlQuery(BusMessageData.TableName);
            var id = QueryTestHelper.ExecuteScalar<int>(lastIdSqlQuery);
            return id;
        }

        public int InsertChunkedServiceRequestMessageData(Guid ownerComponentId, Guid guid, DateTime versionUtc,
            Guid correlationId, byte[] data, bool eof, Guid operation, int order)
        {
            string sqlQuery = String.Format(
                "INSERT INTO [ClientComponent].[{0}]" +
                "([{0}_Operation],[{0}_CorrelationId],[{0}_Order],[{0}_Eof],[{0}_Version],[{0}_ComponentOwner],[{0}_Data])"
                + "VALUES ('{1}','{2}',{3},{4},{5},'{6}',@binaryValue)",ChunkedServiceRequestMessageData.TableName,operation, correlationId,order,eof?1:0,versionUtc.Ticks,ownerComponentId);
            QueryTestHelper.ExecuteNonQuery(sqlQuery,new Tuple<string,byte[]>("@binaryValue",data));
            string lastIdSqlQuery = LastIdSqlQuery(ChunkedServiceRequestMessageData.TableName);
            var id = QueryTestHelper.ExecuteScalar<int>(lastIdSqlQuery);
            return id;
        }

        public int InsertConnectivityDetailsRecord(Guid componentId, Guid componentOwnerId,
                                                      string ip, int port, bool isLocal, Guid serverId)
        {
            return InsertConnectivityDetailsRecord(componentId, componentOwnerId, ip, port, isLocal, serverId,
                                                   DateTime.UtcNow);
        }

        public int InsertConnectivityDetailsRecord(Guid componentId, Guid componentOwnerId,
                                                      string ip, int port, bool isLocal, Guid serverId,
                                                      DateTime versionUtc)
        {
            QueryTestHelper.ExecuteScalar(InsertConnectivityDetailsSqlQuery(componentOwnerId, ip,
                                                                                    port, isLocal, serverId, versionUtc));
            var id = QueryTestHelper.ExecuteScalar<int>(LastIdSqlQuery(ConnectivityDetails.TableName));
            return id;
        }

        private string InsertConnectivityDetailsSqlQuery(Guid componentOwnerId, string ip, int port,
                                                         bool isLocal, Guid serverId, DateTime versionUtc)
        {
            return
                string.Format(
                    "Insert into [ClientComponent].{0} (ConnectivityDetails_Ip,ConnectivityDetails_Port,ConnectivityDetails_ComponentOwner,ConnectivityDetails_IsLocal,{0}_ServerId,{0}_Version) Values ('{1}',{2},'{3}',{4},'{5}',{6})",
                    ConnectivityDetails.TableName, ip, port, componentOwnerId, isLocal ? 1 : 0, serverId,
                    versionUtc.Ticks);
        }

        public int InsertServiceDetails( Guid componentOwner, Guid publisher, string methodName,
                                           string typeName, string interfaceName, Guid operationId, DateTime versionUtc,
                                           bool isSystemService)
        {
            string query =
                string.Format(
                    "Insert into [ClientComponent].{0} ({0}_ComponentOwner, {0}_Publisher,{0}_ServiceImplementationMethodName,{0}_ServiceImplementationTypeName,{0}_ServiceInterfaceTypeName,{0}_OperationIdentifier,{0}_Version,{0}_IsSystemService) Values ('{1}','{2}','{3}','{4}','{7}','{5}',{6},{8})",
                    ServiceDetails.TableName, componentOwner, publisher, methodName, typeName, operationId,
                    versionUtc.Ticks, interfaceName, isSystemService ? 1 : 0);

            QueryHelper queryTestHelper = QueryTestHelper;
            queryTestHelper.ExecuteScalar(query);
            var id = queryTestHelper.ExecuteScalar<int>(LastIdSqlQuery(ServiceDetails.TableName));
            return id;
        }

        public TObject GetObjectFromDb<TObject>( int id, string tableName)
            where TObject : ModelBase, new()
        {
            return
                QueryTestHelper.GetObjectFromRow<TObject>(string.Format("select * from {0}.{1}", SchemaName,
                                                                                tableName));
        }

        public List<TObject> GetObjectsFromDb<TObject>( string tableName)
            where TObject : ModelBase, new()
        {
            var result = new List<TObject>();
            DataTable tableFromDb = GetTableFromDb(tableName);
            foreach (DataRow row in tableFromDb.Rows)
            {
                result.Add(QueryTestHelper.GetObjectFromRow<TObject>(row));
            }
            return result;
        }

        public List<TObject> GetObjectsFromDb<TObject>()
            where TObject : ModelBase, new()
        {
            return GetObjectsFromDb<TObject>(typeof (TObject).Name);
        }

        public DataTable GetTableFromDb(string tableName, int id)
        {
            return
                QueryTestHelper.GetTable(string.Format("select * from {0}.{1} where {1}_Id={2}", SchemaName,
                                                                   tableName, id));
        }

        public DataTable GetTableFromDb(string tableName)
        {
            return
                QueryTestHelper.GetTable(string.Format("select * from {0}.{1}", SchemaName, tableName));
        }


        
    }
}