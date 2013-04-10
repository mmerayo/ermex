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
using System.Linq;
using System.Reflection;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.DAL.Interfaces.QueryDatabase;
using ermeX.Versioning.Schema.Scripts.SqlServer;

namespace ermeX.Versioning.Schema.Scripts
{
	internal abstract class SchemaScriptRunner
	{
		private readonly string _connStr;
		private readonly IList<DataSchemaType> _schemasApplied;

		protected SchemaScriptRunner(DbEngineType engineType, IList<DataSchemaType> schemasApplied, string configurationConnectionString, IQueryHelper queryHelper)
		{
			EngineType = engineType;
			_schemasApplied = schemasApplied;
			_connStr = configurationConnectionString;
			DbQueryHelper = queryHelper;
		}

		protected DbEngineType EngineType { get; private set; }
		protected abstract string NamespaceResourcesPrefix { get; }

		private IQueryHelper DbQueryHelper{get; set; }

		protected internal IList<DataSchemaType> SchemasApplied
		{
			get { return _schemasApplied; }
		}

		protected internal string ConnectionString
		{
			get { return _connStr; }
		}

		public void RunUpgrades()
		{
			var scripts = GetScriptsSortedReadyForExecution();

			foreach (var scriptToRun in scripts)
			{
				ExecuteScript(scriptToRun, false);
			}
		}

		private List<ScriptToRun> GetScriptsSortedReadyForExecution()
		{
			var scripts = new List<ScriptToRun>();
			var resourceNames =
				Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(
					x => x.StartsWith(NamespaceResourcesPrefix) && x.EndsWith(".sql")).ToList();
			foreach (var resourceName in resourceNames)
			{
				var scriptToRun = new ScriptToRun
					{
						ResourceName = resourceName,
						Timestamp = resourceName.Replace(".sql", string.Empty).Split('.').Last(),
					};
				string schemaType =
					resourceName.Replace(".sql", string.Empty).Replace("." + scriptToRun.Timestamp, string.Empty).
					             Replace(NamespaceResourcesPrefix + ".", string.Empty);
				scriptToRun.SchemaType = (DataSchemaType) Enum.Parse(typeof (DataSchemaType), schemaType);
				scripts.Add(scriptToRun);
			}

			scripts = FilterAffectedSchemas(scripts);

			scripts = SortScripts(scripts);

			scripts = ExecuteVersionTableCreationScripts(scripts);

			scripts = FilterAlreadyApplied(scripts);

			return scripts;
		}

		private List<ScriptToRun> FilterAlreadyApplied(List<ScriptToRun> scripts)
		{
			var schemas = scripts.Select(x => x.SchemaType).Distinct().ToList();
			foreach (var dataSchemaType in schemas)
			{
				var top = (string) DbQueryHelper.ExecuteScalar(
					GetLatestVersionExecutedSqlQuery(dataSchemaType));

				if (top != null)
					scripts.RemoveAll(x => DontNeedExecution(x, top, dataSchemaType));
			}
			return scripts;
		}

		protected abstract string GetLatestVersionExecutedSqlQuery(DataSchemaType dataSchemaType);


		private bool DontNeedExecution(ScriptToRun current, string max, DataSchemaType dataSchemaType)
		{
			if (current.SchemaType == dataSchemaType && GetTimeStampDate(current.Timestamp) <= GetTimeStampDate(max))
				return true;
			return false;
		}

		private static DateTime GetTimeStampDate(string timeStamp)
		{
			string year = timeStamp.Substring(0, 4);
			string month = timeStamp.Substring(4, 2);
			string day = timeStamp.Substring(6, 2);
			string hour = timeStamp.Substring(8, 2);
			string minute = timeStamp.Substring(10, 2);
			string second = timeStamp.Substring(12, 2);

			string timestampdate = year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + second;

			return DateTime.Parse(timestampdate);
		}


		private List<ScriptToRun> FilterAffectedSchemas(List<ScriptToRun> scripts)
		{
			return
				scripts.Where(x => x.SchemaType == DataSchemaType.dbo || SchemasApplied.Contains(x.SchemaType)).ToList();
		}

		private List<ScriptToRun> ExecuteVersionTableCreationScripts(List<ScriptToRun> scripts)
		{
			ScriptToRun versionScript = scripts.Single(x => x.Timestamp == "00000000000000");
			ExecuteScript(versionScript, true);
			scripts.Remove(versionScript);

			return scripts;
		}

		private void ExecuteScript(ScriptToRun script, bool isVersionTableCreation)
		{
			var tSql = ScriptUtils.GetSqlFromResource(script.ResourceName);
			var sqlCommands = new List<string>(ScriptUtils.SplitScriptsByGoKeyword(tSql));
			if (!isVersionTableCreation)
				sqlCommands.Add(GetVersionUpdateCommand(script));
			DbQueryHelper.ExecuteBatchNonQuery(sqlCommands.ToArray());
		}

		private string GetVersionUpdateCommand(ScriptToRun script)
		{
			string result =
				string.Format(
					"INSERT INTO [dbo].[Version]  ([Version_TimeStamp] ,[Version_SchemaType] ,[Version_DateApplied])" +
					" VALUES ('{0}',{1},'{2}')", script.Timestamp, (int) script.SchemaType,
					DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss"));

			return result;
		}


		private List<ScriptToRun> SortScripts(List<ScriptToRun> result)
		{
			return result.OrderBy(x => x.Timestamp).ToList();
		}

		//TODO: THIS SHOULD BE INJECTED
		public static SchemaScriptRunner GetRunner(IList<DataSchemaType> schemasApplied,
		                                           string configurationConnectionString,
		                                           DbEngineType configurationSourceType,
		                                           IQueryHelper queryHelper)
		{
			SchemaScriptRunner result;
			switch (configurationSourceType)
			{
				case DbEngineType.SqlServer2008:
					result = new SqlServerSchemaScriptRunner(schemasApplied, configurationConnectionString, queryHelper);
					break;
				case DbEngineType.Sqlite:
				case DbEngineType.SqliteInMemory:
					result = new SqliteSchemaScriptRunner(schemasApplied, configurationConnectionString, queryHelper);
					break;
				default:
					throw new NotImplementedException();
			}
			return result;
		}

		#region Nested type: ScriptToRun

		private struct ScriptToRun
		{
			public string ResourceName { get; set; }
			public string Timestamp { get; set; }
			public DataSchemaType SchemaType { get; set; }
		}

		#endregion
	}
}