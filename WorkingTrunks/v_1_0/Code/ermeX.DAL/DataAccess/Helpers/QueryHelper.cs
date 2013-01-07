// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Entities.Base;

namespace ermeX.DAL.DataAccess.Helpers
{
    /// <summary>
    ///   Use this only for the versioning and the tests
    /// </summary>
    internal abstract class QueryHelper
    {
        private readonly string _connectionString;

        protected QueryHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public static QueryHelper GetHelper(DbEngineType type, string connectionString)
        {
            QueryHelper result = null;
            switch (type)
            {
                case DbEngineType.SqlServer2008:
                    result = new Sql2008DbQueryHelper(connectionString);
                    break;
                case DbEngineType.SqliteInMemory:
                case DbEngineType.Sqlite:
                    result = new SqliteDbQueryHelper(connectionString);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
            return result;
        }

        protected abstract DbConnection GetConnection();
        protected abstract DbDataAdapter GetDataAdapter(string sqlQuery, DbConnection conn);
        protected abstract string GetSupportedSql(string sqlQuery);
        public abstract string GetLastIdQuery(string schemaName, string tableName, string idFieldName);

        public DataTable GetTable(string sqlQuery)
        {
            var result = new DataTable();
            using (var conn = GetConnection())
            {
                using (var da = GetDataAdapter(GetSupportedSql(sqlQuery), conn))
                {
                    conn.Open();
                    da.Fill(result);
                }
                conn.Close();
            }
            return result;
        }

        internal bool MainDbDefinitionsExist(IDalSettings settingsSource)
        {
            bool result = (!SupportsProcedures || StoredProceduresExist()) && TablesExist() &&
                          (!SupportsSchemas || SchemasExist(settingsSource.SchemasApplied));

            return result;
        }

        protected abstract bool SupportsSchemas { get;}
        protected abstract bool SupportsProcedures { get; }

        private bool SchemasExist(IList<DataSchemaType> schemasApplied)
        {
            if (!SupportsSchemas) throw new InvalidOperationException();

            foreach (var itemName in schemasApplied)
            {
                if (!SchemaExist(itemName.ToString())) return false;
            }
            return true;
        }

        private bool SchemaExist(string itemName)
        {
            if (!SupportsSchemas) throw new InvalidOperationException();

            var existSchemaSql = GetExistSchemaSql(itemName);

            if (!string.IsNullOrEmpty(existSchemaSql) && (int) ExecuteScalar(existSchemaSql) == 0)
                return false;
            return true;
        }


        protected abstract string GetExistSchemaSql(string itemName);


        private bool TablesExist()
        {
            List<string> itemNames = GetTableNamesSortedFromLeafsDependencies();

            foreach (var itemName in itemNames)
            {
                if (!TableExist(itemName)) return false;
            }
            return true;
        }

        private bool TableExist(string itemName)
        {
            var existTableSql = GetExistTableSql(itemName);
            object executeScalar = ExecuteScalar<int?>(existTableSql);
            if (executeScalar == null) executeScalar = 0;
            if (!string.IsNullOrEmpty(existTableSql) && (int) executeScalar == 0)
                return false;
            return true;
        }

        protected abstract string GetExistTableSql(string itemName);

        private bool StoredProceduresExist()
        {
            if(!SupportsProcedures)
                throw new InvalidOperationException("unsupported");

            List<string> itemNames = GetTableNamesSortedFromLeafsDependencies();

            foreach (var itemName in itemNames)
            {
                if (!StoredProcedureExist(itemName)) return false;
            }
            return true;
        }

        private bool StoredProcedureExist(string itemName)
        {
            if (!SupportsProcedures)
                throw new InvalidOperationException("unsupported");
            var existSpSql = GetExistSpSql(itemName);
            if (!string.IsNullOrEmpty(existSpSql) && (int) ExecuteScalar(existSpSql) == 0)
                return false;
            return true;
        }

        protected abstract string GetExistSpSql(string itemName);

        public void DeleteDbDefinitions()
        {
            DeleteStoredProcedures();
            DeleteTables();
            DeleteSchemas();
        }


        private void DeleteSchemas()
        {
            if(!SupportsSchemas) return;

            List<string> itemNames = GetSchemaNames().Where(x => x != DataSchemaType.dbo.ToString()).ToList();

            foreach (var itemName in itemNames)
            {
                if (SchemaExist(itemName))
                    ExecuteNonQuery(GetDeleteSchemaSql(itemName));
            }
        }

        protected abstract string GetDeleteSchemaSql(string itemName);


        private void DeleteTables()
        {
            List<string> itemNames = GetTableNamesSortedFromLeafsDependencies();

            foreach (var itemName in itemNames)
            {
                if (TableExist(itemName))
                    ExecuteNonQuery(GetDeleteTableSql(itemName));
            }
        }

        protected abstract string GetDeleteTableSql(string itemName);


        private void DeleteStoredProcedures()
        {
            if(!SupportsProcedures) return;

            List<string> itemNames = GetStoredProcedureNames();

            foreach (var itemName in itemNames)
            {
                if (StoredProcedureExist(itemName))
                    ExecuteNonQuery(GetDeleteSpSql(itemName));
            }
        }

        protected abstract string GetDeleteSpSql(string itemName);

        private List<string> GetSchemaNames()
        {
            return new List<string>(Enum.GetNames(typeof (DataSchemaType)));
        }

        protected abstract List<string> GetTableNamesSortedFromLeafsDependencies();

        

        public void RemoveAllData()
        {
            var tables = GetTableNamesSortedFromLeafsDependencies().Where(x => x != "Version").ToList();

            foreach (var table in tables)
            {
                if (TableExist(table))
                {
                    string sqlQuery = GetSupportedSql(string.Format("delete from {0}", table));
                    ExecuteNonQuery(sqlQuery);
                }
            }
        }

        private List<string> GetStoredProcedureNames()
        {
            return new List<string>(); //TODO:Update on demand
        }

        public int ExecuteNonQuery(string sqlQuery)
        {
            int result = 0;

            if (!string.IsNullOrEmpty(sqlQuery))
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    using (var tr = conn.BeginTransaction())
                    {
                        try
                        {
                            using (var command = conn.CreateCommand())
                            {
                                command.Transaction = tr;
                                command.CommandText = GetSupportedSql(sqlQuery);
                                result = command.ExecuteNonQuery();
                            }
                            tr.Commit();
                        }
                        catch(Exception ex)
                        {
                            tr.Rollback();
                            throw ex;
                        }
                    }

                    conn.Close();
                }
            }
            return result;
        }

        public int ExecuteNonQuery(string sqlQuery, Tuple<string, byte[]> binaryValue)
        {
            if (binaryValue == null) throw new ArgumentNullException("binaryValue");
            int result = 0;
            if (!string.IsNullOrEmpty(sqlQuery))
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    using (var tr = conn.BeginTransaction())
                    {
                        try
                        {
                            using (var command = conn.CreateCommand())
                            {
                                command.Transaction = tr;
                                command.CommandText = GetSupportedSql(sqlQuery);
                                AddBinaryParameterToCommand(command,binaryValue.Item1,binaryValue.Item2);
                                result = command.ExecuteNonQuery();
                            }
                            tr.Commit();
                        }
                        catch (Exception ex)
                        {
                            tr.Rollback();
                            throw ex;
                        }
                    }

                    conn.Close();
                }
            }
            return result;
        }

        protected abstract void AddBinaryParameterToCommand(DbCommand command, string item1, byte[] item2);

        public void ExecuteBatchNonQuery(string[] sqlCommands)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (DbTransaction tr = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var sqlQuery in sqlCommands)
                        {
                            if (!string.IsNullOrEmpty(sqlQuery))
                            {
                                using (var command = conn.CreateCommand())
                                {
                                    command.Transaction = tr;
                                    command.CommandText = GetSupportedSql(sqlQuery);
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                }
                conn.Close();
            }
        }

        public TResult ExecuteScalar<TResult>(string sqlQuery)
        {
            object executeScalar = ExecuteScalar(sqlQuery);
            if (typeof(TResult) == typeof(Int32) || (typeof(TResult)==typeof(int?) && executeScalar!=null))
                executeScalar = Convert.ToInt32(executeScalar);

            return (TResult) executeScalar;
            //return unchecked((TResult)executeScalar);
        }

        public object ExecuteScalar(string sqlQuery)
        {
            object result = null;

            if (!string.IsNullOrEmpty(sqlQuery))
            {
                using (
                    var conn = GetConnection())
                {
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = GetSupportedSql(sqlQuery);
                        conn.Open();
                        result = command.ExecuteScalar();
                    }
                    conn.Close();
                }
            }
            return result;
        }

        public TObject GetObjectFromRow<TObject>(string sqlQuery) where TObject : ModelBase, new()
        {
            var dataTable = GetTable(sqlQuery);
            if (dataTable.Rows.Count == 0)
                return null;
            if (dataTable.Rows.Count > 1)
                throw new Exception("There are more than one row");
            DataRow dataRow = dataTable.Rows[0];
            return ObjectFromRow<TObject>(dataRow);
        }

        public TObject GetObjectFromRow<TObject>(DataRow row) where TObject : ModelBase, new()
        {
            return ObjectFromRow<TObject>(row);
        }


        public static TObject ObjectFromRow<TObject>(DataRow row) where TObject : ModelBase, new()
        {
            if (row == null) throw new ArgumentNullException("row");


            Type methodType = typeof (TObject);

            const string name = "FromDataRow";

            MethodInfo methodInfo = methodType.GetMethod(name,
                                                         BindingFlags.Static | BindingFlags.Public);


            var parameters = new[] {row};
            var result = (TObject)TypesHelper.InvokeFast(methodInfo,null, parameters);//TODO: INVESTIGATE & FIX why FastInvoke wont work on x86 THROWING "JIT Compiler encountered an internal limitation" 
            return result;
        }

        public static List<TObject> ObjectsFromTable<TObject>(DataTable table) where TObject : ModelBase, new()
        {
            if (table == null) throw new ArgumentNullException("table");

            var result = new List<TObject>();

            foreach (DataRow row in table.Rows)
            {
                result.Add(ObjectFromRow<TObject>(row));
            }

            return result;
        }

        #region sqlserver file

        private class Sql2008DbQueryHelper : QueryHelper
        {
            public Sql2008DbQueryHelper(string connectionString) : base(connectionString)
            {
            }

            protected override DbDataAdapter GetDataAdapter(string sqlQuery, DbConnection conn)
            {
                return new SqlDataAdapter(sqlQuery, (SqlConnection) conn);
            }

            protected override string GetSupportedSql(string sqlQuery)
            {
                return sqlQuery;
            }

            protected override bool SupportsSchemas
            {
                get { return true; }
            }

            protected override bool SupportsProcedures
            {
                get { return true; }
            }

            protected override DbConnection GetConnection()
            {
                return new SqlConnection(ConnectionString);
            }

            protected override List<string> GetTableNamesSortedFromLeafsDependencies()
            {
                //TODO:Query db

                return new List<string>
                       {
                           "Version",
                           "ClientComponent.ChunkedServiceRequestMessages",
                           "ClientComponent.BusMessages",

                           "ClientComponent.OutgoingMessageSuscriptions",
                           "ClientComponent.IncomingMessageSuscriptions",

                           "ClientComponent.IncomingMessages",
                           "ClientComponent.OutgoingMessages",
                           "ClientComponent.ConnectivityDetails",
                           "ClientComponent.Components",
                           "ClientComponent.ServicesDetails",
                       }; 
            }

            protected override void AddBinaryParameterToCommand(DbCommand command, string binaryParamName, byte[] value)
            {
                var cmd = (SqlCommand) command;
                cmd.Parameters.Add("@binaryValue", SqlDbType.VarBinary, 8000).Value = value;
            }

            protected override string GetExistSchemaSql(string itemName)
            {
                return
                    string.Format(
                        "SELECT case when exists(select * from sys.schemas where name = '{0}') then 1 else 0 end",
                        itemName);
            }

            protected override string GetExistTableSql(string itemName)
            {
                return string.Format("SELECT case when object_id('{0}')is not null then 1 else 0 end", itemName);
            }

            protected override string GetExistSpSql(string itemName)
            {
                return string.Format("SELECT case when object_id('{0}')is not null then 1 else 0 end", itemName); //??
            }

            protected override string GetDeleteSchemaSql(string itemName)
            {
                return string.Format("DROP SCHEMA {0}", itemName);
            }

            protected override string GetDeleteTableSql(string itemName)
            {
                return string.Format("DROP TABLE {0}", itemName);
            }

            protected override string GetDeleteSpSql(string itemName)
            {
                return string.Format("DROP PROCEDURE {0}", itemName);
            }

            public override string GetLastIdQuery(string schemaName, string tableName, string idFieldName)
            {
                return string.Format("Select max({0}) FROM {1}.{2}", idFieldName, schemaName, tableName);
            }
        }

        #endregion

        //basically overrides getting the accepted sql
        private class SqliteDbQueryHelper : Sql2008DbQueryHelper
        {
            private static readonly List<Regex> Removals = new List<Regex>();
            private static readonly List<Tuple<Regex, string>> Replacements = new List<Tuple<Regex, string>>();
            static SqliteDbQueryHelper()
            {

                Removals.Add(new Regex("\\bclustered\\b", RegexOptions.IgnoreCase));
                Removals.Add(new Regex("\\bnonclustered\\b", RegexOptions.IgnoreCase));
                Removals.Add(new Regex("\\[dbo\\].", RegexOptions.IgnoreCase));
                Removals.Add(new Regex("\\[ClientComponent\\].", RegexOptions.IgnoreCase));
                Removals.Add(new Regex("\\bdbo.", RegexOptions.IgnoreCase));
                Removals.Add(new Regex("\\bClientComponent.", RegexOptions.IgnoreCase));

                Replacements.Add(
                    new Tuple<Regex,string> (new Regex(@"\bbigint\s*identity\s*\(\s*\d\s*,\s*\d\s*\)\s*primary\s*key\b", RegexOptions.IgnoreCase),
                    " INTEGER primary key autoincrement "));
                Replacements.Add(
                    new Tuple<Regex,string> (new Regex(@"\bbigint\s*identity\s*\(\s*\d\s*,\s*\d\s*\)", RegexOptions.IgnoreCase),
                    " INTEGER primary key autoincrement "));
                Replacements.Add(new Tuple<Regex,string> (new Regex(@"varbinary\(MAX\)", RegexOptions.IgnoreCase), "BLOB"));
                Replacements.Add(new Tuple<Regex,string> (new Regex(@"nvarchar\s*\(\s*MAX\s*\)", RegexOptions.IgnoreCase), "TEXT"));
            }

            public SqliteDbQueryHelper(string connectionString)
                : base(connectionString)
            {
            }

            protected override List<string> GetTableNamesSortedFromLeafsDependencies()
            {
                List<string> result = base.GetTableNamesSortedFromLeafsDependencies();
                for (int index = 0; index < result.Count; index++)
                {
                    var item = result[index];
                    result[index] = item.Replace("ClientComponent.", string.Empty);
                }
                return result;
            }

            protected override string GetSupportedSql(string sqlQuery)
            {
                foreach (var removal in Removals)
                {
                    sqlQuery = removal.Replace(sqlQuery, string.Empty);
                }
                foreach (var replacement in Replacements)
                {
                    sqlQuery = replacement.Item1.Replace(sqlQuery, replacement.Item2);
                }

                return sqlQuery;
            }


            protected override DbDataAdapter GetDataAdapter(string sqlQuery, DbConnection conn)
            {
                return new SQLiteDataAdapter(GetSupportedSql(sqlQuery), (SQLiteConnection)conn);
            }

            protected override bool SupportsSchemas
            {
                get { return false; }
            }

            protected override bool SupportsProcedures
            {
                get { return false; }
            }

            protected override DbConnection GetConnection()
            {
                return new SQLiteConnection(ConnectionString);
            }

            protected override string GetExistSchemaSql(string itemName)
            {
                throw new InvalidOperationException("Schemas not supported");
            }

            protected override string GetExistTableSql(string itemName)
            {
                return
                    string.Format(
                        "SELECT CASE WHEN tbl_name = \"{0}\" THEN 1 ELSE 0 END FROM sqlite_master WHERE tbl_name = \"{0}\" AND type = \"table\"",
                        itemName);
            }

            protected override string GetExistSpSql(string itemName)
            {
                throw new InvalidOperationException("SPs not supported");
            }

            protected override string GetDeleteSchemaSql(string itemName)
            {
                throw new InvalidOperationException("Schemas not supported");
            }

            protected override string GetDeleteTableSql(string itemName)
            {
                return string.Format("DROP TABLE IF EXISTS {0}", itemName);
            }

            protected override string GetDeleteSpSql(string itemName)
            {
                throw new InvalidOperationException("SPs not supported");
            }
            public override string GetLastIdQuery(string schemaName, string tableName, string idFieldName)
            {
                return string.Format("Select MAX({0}) FROM {1}", idFieldName,  tableName);
            }

            protected override void AddBinaryParameterToCommand(DbCommand command, string binaryParamName, byte[] value)
            {
                var cmd = (SQLiteCommand)command;
                cmd.Parameters.Add("@binaryValue",DbType.Binary).Value = value;
            }
        }

       
    }
}