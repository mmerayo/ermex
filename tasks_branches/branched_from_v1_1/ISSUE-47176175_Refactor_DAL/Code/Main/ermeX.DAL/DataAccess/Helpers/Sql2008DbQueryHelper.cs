using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace ermeX.DAL.DataAccess.Helpers
{
	internal class Sql2008DbQueryHelper : QueryHelper
	{
		public Sql2008DbQueryHelper(string connectionString)
			: base(connectionString)
		{
		}

		protected override DbDataAdapter GetDataAdapter(string sqlQuery, DbConnection conn)
		{
			return new SqlDataAdapter(sqlQuery, (SqlConnection)conn);
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
			var cmd = (SqlCommand)command;
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
}