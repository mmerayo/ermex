// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Data.SqlClient;
using System.Threading;

namespace ermeX.Tests.Common.DataAccess
{
    internal sealed class SqlServerDbEngine : ITestDbEngine,IDisposable
    {
        private readonly object _syncLock=new object();

        
        private string DbName { get; set; }

        public SqlServerDbEngine(string dbName)
        {
            if(string.IsNullOrEmpty(dbName ))
                throw new ArgumentException("dbName");
            DbName = string.Format("ermeX_Test_{0}", dbName.Replace('-', '_'));
        }

        public void CreateDatabase()
        {
            using (var conn = new SqlConnection(DbTestEnvironment.Instance.GetMasterConnectionString()))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 60;
                    cmd.CommandText = string.Format("SELECT count(1) FROM sys.databases WHERE name = N'{0}'", DbName);
                    conn.Open();
                    int result = Convert.ToInt32(cmd.ExecuteScalar());

                    if (result > 0)
                    {
                        DropDatabase(conn);
                    }

                    cmd.CommandText = string.Format("CREATE database {0}", DbName);
                    cmd.Dispose();

                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }

            SqlConnection.ClearAllPools();

            Thread.Sleep(50);//TODO: REMOVE AND TEST
        }

        public void DropDatabase()
        {
            DropDatabase(null);
        }

        private void DropDatabase(SqlConnection masterConnection)
        {
            if (string.IsNullOrEmpty(DbName))
                return;

            bool mustDispose = false;
            if (masterConnection == null)
            {
                masterConnection = new SqlConnection(DbTestEnvironment.Instance.GetMasterConnectionString());
                mustDispose = true;
                masterConnection.Open();
            }

            using (var cmd = masterConnection.CreateCommand())
            {
                cmd.CommandText =
                    string.Format("IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') drop database {0};",
                                  DbName);

                DisconnectAllUsersAndSetDbToSingleUserMode(masterConnection);
                Thread.Sleep(50);

                cmd.ExecuteNonQuery();
            }
            if (mustDispose)
            {
                masterConnection.Close();
                masterConnection.Dispose();
            }
        }

        private void DisconnectAllUsersAndSetDbToSingleUserMode(SqlConnection masterConn)
        {
            if (string.IsNullOrEmpty(DbName)) return;
            var sql = string.Format("IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') " +
                                    "ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", DbName);
            using (SqlCommand cmd = masterConn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        public string GetConnectionString()
        {
            return DbTestEnvironment.Instance.GetConnectionString(DbName);
        }

        private bool _disposed = false;
        public void Dispose()
        {
            if(!_disposed)
                lock (_syncLock) 
                    if (!_disposed)
                    {
                        DropDatabase();
                        _disposed = true;
                    }
            
        }

        #region Nested type: DbTestEnvironment
        //TODO: REFACTOR IN ONE CLASS WITH THE NEXT
        private class DbTestEnvironment
        {
            private static volatile DbTestEnvironment _instance;
            private static readonly object _locker = new object();

            public static DbTestEnvironment Instance
            {
                get
                {
                    if (_instance == null)
                        lock (_locker)
                            if (_instance == null)
                            {
                                var server = Environment.GetEnvironmentVariable("TEST_SQL_SERVER_ADDRESS") ??
                                             "localhost\\SQLExpress";
                                var adminLogin =
                                    Environment.GetEnvironmentVariable("TEST_SQL_SERVER_ADMIN_LOGIN") ?? "sa";
                                var adminPassword =
                                    Environment.GetEnvironmentVariable("TEST_SQL_SERVER_ADMIN_PASSWORD") ??
                                    "sqlsql";
                                _instance = new DbTestEnvironment(server, adminLogin, adminPassword);
                            }


                    return _instance;
                }
            }

            private DbTestEnvironment(string serverAddress, string adminLogin, string adminPassword)
            {
                ServerAddress = serverAddress;
                AdminLogin = adminLogin;
                AdminPassword = adminPassword;
            }

            public string ServerAddress { get; private set; }

            public string AdminLogin { get; private set; }

            public string AdminPassword { get; private set; }

            public string GetMasterConnectionString()
            {
                return string.Format("Data Source={0};Initial Catalog=Master;User ID={1};PWD={2};Enlist=false;Connection Timeout=60",
                                     ServerAddress, AdminLogin, AdminPassword);
            }

            public string GetConnectionString(string dbName)
            {
                if (string.IsNullOrEmpty(dbName)) throw new ArgumentNullException("dbName");
                return string.Format("Data Source={0};User ID={1};PWD={2};Enlist=false;Initial Catalog={3};Connection Timeout=60",
                                     ServerAddress, AdminLogin, AdminPassword, dbName);
            }
        }

        #endregion

        
    }
}