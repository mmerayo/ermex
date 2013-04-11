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
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ermeX.Common;

namespace ermeX.Tests.Common.DataAccess
{
	internal class SqliteDbEngine : ITestDbEngine, IDisposable
	{
		protected bool InMemory { get; private set; }
		private readonly object _syncLock = new object();

		private SQLiteConnection _inMemoryConnection = null; //holds the connection in memory

		private SqliteConnectionProvider DataBase { get; set; }

		public SqliteDbEngine(string dbName, bool inMemory, string dbFolderPath)
		{
			if (string.IsNullOrEmpty(dbName))
				throw new ArgumentException("dbName");
			InMemory = inMemory;

			DataBase = new SqliteConnectionProvider(string.Format("db{0}", dbName.Replace('-', '_')), inMemory, dbFolderPath);
		}

		public SqliteDbEngine(string dbName, bool inMemory = false)
			: this(dbName, inMemory, null)
		{

		}


		public void CreateDatabase()
		{
			//its created automatically
			//DropDatabase(); //to keep the same behavior as sqlserver //TODO: THIS MIGHT NEED TO BE REMOVED

			if (InMemory)
			{
				lock (_syncLock)
				{
					Debug.Assert(_inMemoryConnection == null);
					_inMemoryConnection = new SQLiteConnection(DataBase.ConnectionString);
					_inMemoryConnection.Open();
				}
			}
		}

		public void DropDatabase()
		{
			lock (_syncLock)
			{
				if (!InMemory)
				{
					DeleteDbFile(3);
					//DataBase = null;

				}
				else
				{
					if (_inMemoryConnection != null)
					{
						_inMemoryConnection.Dispose();
						_inMemoryConnection = null;
					}
				}
			}
		}

		private void DeleteDbFile(int tries)
		{
			if (tries == 0)
				return;
			try
			{
				if (File.Exists(DataBase.FilePath))
					File.Delete(DataBase.FilePath);
			}
			catch (Exception)
			{
				Thread.Sleep(150);
				DeleteDbFile(--tries);
			}
		}

		public string GetConnectionString()
		{
			return DataBase.ConnectionString;
		}

		private bool _disposed = false;

		public void Dispose()
		{
			if (!_disposed)
				lock (_syncLock)
					if (!_disposed)
					{
						DropDatabase();
						_disposed = true;
					}

		}

		private class SqliteConnectionProvider
		{
			private readonly bool _inMemory;
			public string DbName { get; private set; }

			public string FilePath { get; private set; }

			public string ConnectionString
			{
				get
				{
					return !_inMemory
						       ? string.Format("Data Source={0};Version=3;BinaryGuid=False;", FilePath)
						       : string.Format("FullUri=file:{0}?mode=memory&cache=shared;Version=3;BinaryGuid=False;", DbName);
				}
			}

			public SqliteConnectionProvider(string dbName, bool inMemory, string dbFolderPath)
			{
				_inMemory = inMemory;
				DbName = dbName;
				if (!inMemory)
					if (string.IsNullOrEmpty(dbFolderPath))
						FilePath = PathUtils.GetApplicationFolderPathFile(PathUtils.GetApplicationFolderPathFile(dbName)) +
						           ".db";
					else
						FilePath = Path.Combine(dbFolderPath, dbName + ".db");

			}
		}
	}
}