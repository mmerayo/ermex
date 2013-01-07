// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ermeX.Common;

namespace ermeX.Tests.Common.DataAccess
{
    class SqliteDbEngine : ITestDbEngine, IDisposable
    {
        protected bool InMemory { get; private set; }
        private readonly object _syncLock=new object();

        private SQLiteConnection _inMemoryConnection = null;//holds the connection in memory
        
        private SqliteConnectionProvider DataBase { get; set; }

        public SqliteDbEngine(string dbName, bool inMemory, string dbFolderPath)
        {
            if (string.IsNullOrEmpty(dbName))
                throw new ArgumentException("dbName");
            InMemory = inMemory;

            DataBase = new SqliteConnectionProvider(string.Format("db{0}", dbName.Replace('-', '_')), inMemory,dbFolderPath);
        }

        public SqliteDbEngine(string dbName, bool inMemory = false):this(dbName,inMemory,null)
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
                if(!InMemory)
                {
                    DeleteDbFile();
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

        private void DeleteDbFile()
        {
            if (File.Exists(DataBase.FilePath))
                File.Delete(DataBase.FilePath);
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
                    if(string.IsNullOrEmpty(dbFolderPath))
                        FilePath = PathUtils.GetApplicationFolderPathFile(PathUtils.GetApplicationFolderPathFile(dbName)) +
                               ".db";
                    else
                        FilePath = Path.Combine(dbFolderPath, dbName+".db");

            }
        }
    }
}
