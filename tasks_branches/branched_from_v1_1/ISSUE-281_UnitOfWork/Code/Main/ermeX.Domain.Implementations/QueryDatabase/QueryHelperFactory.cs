﻿using System;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.DAL.Interfaces;
using ermeX.Domain.QueryDatabase;

namespace ermeX.Domain.Implementations.QueryDatabase
{
	class QueryHelperFactory : IQueryHelperFactory
	{
		public IQueryHelper GetHelper(DbEngineType type, string connectionString)
		{
			IQueryHelper result = null;
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
	}
}