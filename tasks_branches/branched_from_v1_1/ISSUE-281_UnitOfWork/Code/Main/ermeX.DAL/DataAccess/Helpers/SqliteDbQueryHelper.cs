using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace ermeX.DAL.DataAccess.Helpers
{
	internal class SqliteDbQueryHelper : Sql2008DbQueryHelper
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
				new Tuple<Regex, string>(new Regex(@"\bbigint\s*identity\s*\(\s*\d\s*,\s*\d\s*\)\s*primary\s*key\b", RegexOptions.IgnoreCase),
				                         " INTEGER primary key autoincrement "));
			Replacements.Add(
				new Tuple<Regex, string>(new Regex(@"\bbigint\s*identity\s*\(\s*\d\s*,\s*\d\s*\)", RegexOptions.IgnoreCase),
				                         " INTEGER primary key autoincrement "));
			Replacements.Add(new Tuple<Regex, string>(new Regex(@"varbinary\(MAX\)", RegexOptions.IgnoreCase), "BLOB"));
			Replacements.Add(new Tuple<Regex, string>(new Regex(@"nvarchar\s*\(\s*MAX\s*\)", RegexOptions.IgnoreCase), "TEXT"));
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
			return string.Format("Select MAX({0}) FROM {1}", idFieldName, tableName);
		}

		protected override void AddBinaryParameterToCommand(DbCommand command, string binaryParamName, byte[] value)
		{
			var cmd = (SQLiteCommand)command;
			cmd.Parameters.Add("@binaryValue", DbType.Binary).Value = value;
		}
	}
}