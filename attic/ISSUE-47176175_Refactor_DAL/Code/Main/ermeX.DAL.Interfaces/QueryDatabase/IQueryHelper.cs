using System;
using System.Data;
using ermeX.ConfigurationManagement.Settings;
using ermeX.Models.Base;

namespace ermeX.DAL.Interfaces.QueryDatabase
{
	//TODO: TO BE REMOVED, AND ONLY USED BY THE VERSION UPGRADER
	internal interface IQueryHelper
	{
		string ConnectionString { get; }
		string GetLastIdQuery(string schemaName, string tableName, string idFieldName);
		DataTable GetTable(string sqlQuery);
		void DeleteDbDefinitions();
		void RemoveAllData();
		int ExecuteNonQuery(string sqlQuery);
		int ExecuteNonQuery(string sqlQuery, Tuple<string, byte[]> binaryValue);
		void ExecuteBatchNonQuery(string[] sqlCommands);
		TResult ExecuteScalar<TResult>(string sqlQuery);
		object ExecuteScalar(string sqlQuery);
		TObject GetObjectFromRow<TObject>(string sqlQuery) where TObject : ModelBase, new();
		TObject GetObjectFromRow<TObject>(DataRow row) where TObject : ModelBase, new();
		bool MainDbDefinitionsExist(IDalSettings settingsSource);
	}
}