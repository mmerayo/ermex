using System;
using System.IO;
using System.Linq;
using NHibernate;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;

namespace ermeX.DAL.Transactions
{
	//due to sqlite issues when multithreading and parallel connections
	internal sealed class MutexedTransactionProvider : IWriteTransactionProvider
	{
		private string _dbName;

		[Inject]
		public MutexedTransactionProvider(IDalSettings settings )
		{
			string connStr = settings.ConfigurationConnectionString;

			switch (settings.ConfigurationSourceType)
			{
				case DbEngineType.Sqlite:
				case DbEngineType.SqliteInMemory:
					if (connStr.Contains("FullUri"))
						ParseInMemorySqliteDb(connStr);
					else if (connStr.Contains("Data Source"))
						ParsePersistentSqliteDb(connStr);
					else
					{
						throw new NotSupportedException();
					}
					break;
				default:
					throw new NotSupportedException();
			}

		}

		private void ParsePersistentSqliteDb(string connStr)
		{
			string dbPathName = connStr.Split(';').Single(x => x.Contains("Data Source")).Split('=')[1];

			_dbName = Path.GetFileName(dbPathName);
		}

		private void ParseInMemorySqliteDb(string connStr)
		{
			var seed = connStr.Split(';').Single(x => x.Contains("FullUri")).Split('=')[1];
			_dbName=seed.Split('?')[0].Split(':')[1];
		}

		public IErmexTransaction BeginTransaction(ITransaction innerTransaction)
		{
			return new MutexedTransaction(_dbName,innerTransaction);
		}
	}
}