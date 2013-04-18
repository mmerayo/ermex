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
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Common.Logging;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.QueryDatabase;
using ermeX.Entities.Base;

namespace ermeX.DAL.DataAccess.Helpers
{
	/// <summary>
	/// Use this only for the versioning and the tests
	/// </summary>
	internal abstract class QueryHelper : IQueryHelper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(QueryHelper).FullName);
		private readonly string _connectionString;

		protected QueryHelper(string connectionString)
		{
			_connectionString = connectionString;
		}

		public string ConnectionString
		{
			get { return _connectionString; }
		}


		protected abstract DbConnection GetConnection();
		protected abstract DbDataAdapter GetDataAdapter(string sqlQuery, DbConnection conn);
		protected abstract string GetSupportedSql(string sqlQuery);
		public abstract string GetLastIdQuery(string schemaName, string tableName, string idFieldName);

		public DataTable GetTable(string sqlQuery)
		{
			Logger.DebugFormat("GetTable. Query: {0}", sqlQuery);
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

		public bool MainDbDefinitionsExist(IDalSettings settingsSource)
		{
			bool result = (!SupportsProcedures || StoredProceduresExist()) && TablesExist() &&
			              (!SupportsSchemas || SchemasExist(settingsSource.SchemasApplied));

			return result;
		}

		protected abstract bool SupportsSchemas { get; }
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
			if (!SupportsProcedures)
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
			if (!SupportsSchemas) return;

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
			if (!SupportsProcedures) return;

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
			Logger.Debug("RemoveAllData");

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
			Logger.DebugFormat("ExecuteNonQuery. Query {0}", sqlQuery);

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

		public int ExecuteNonQuery(string sqlQuery, Tuple<string, byte[]> binaryValue)
		{
			Logger.DebugFormat("ExecuteNonQuery. with binaryValue Query {0}", sqlQuery);

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
								AddBinaryParameterToCommand(command, binaryValue.Item1, binaryValue.Item2);
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
			Logger.DebugFormat("ExecuteBatchNonQuery. Queries {0}", string.Concat(sqlCommands));

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
			Logger.DebugFormat("ExecuteScalar. Query {0}", sqlQuery);
			object executeScalar = ExecuteScalar(sqlQuery);
			if (typeof (TResult) == typeof (Int32) || (typeof (TResult) == typeof (int?) && executeScalar != null))
				executeScalar = Convert.ToInt32(executeScalar);

			return (TResult) executeScalar;
			//return unchecked((TResult)executeScalar);
		}

		public object ExecuteScalar(string sqlQuery)
		{
			Logger.DebugFormat("ExecuteScalar. Query {0}", sqlQuery);
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
			Logger.DebugFormat("GetObjectFromRow. Query {0}", sqlQuery);
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
			Logger.Debug("GetObjectFromRow. DataRow ");
			return ObjectFromRow<TObject>(row);
		}


		public static TObject ObjectFromRow<TObject>(DataRow row) where TObject : ModelBase, new()
		{
			Logger.Debug("ObjectFromRow. DataRow ");
			if (row == null) throw new ArgumentNullException("row");


			Type methodType = typeof (TObject);

			const string name = "FromDataRow";

			MethodInfo methodInfo = methodType.GetMethod(name,
			                                             BindingFlags.Static | BindingFlags.Public);


			var parameters = new[] {row};
			var result = (TObject) TypesHelper.InvokeFast(methodInfo, null, parameters);
			//TODO: INVESTIGATE & FIX why FastInvoke wont work on x86 THROWING "JIT Compiler encountered an internal limitation" 
			return result;
		}

		public static List<TObject> ObjectsFromTable<TObject>(DataTable table) where TObject : ModelBase, new()
		{
			Logger.Debug("ObjectsFromTable. DataTable");
			if (table == null) throw new ArgumentNullException("table");

			var result = new List<TObject>();

			foreach (DataRow row in table.Rows)
			{
				result.Add(ObjectFromRow<TObject>(row));
			}

			return result;
		}
	}
}