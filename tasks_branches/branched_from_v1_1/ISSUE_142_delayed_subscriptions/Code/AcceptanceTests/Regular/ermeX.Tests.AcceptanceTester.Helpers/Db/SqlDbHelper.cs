using System;
using System.Data.SqlClient;
using System.Threading;

namespace ermeX.Tests.AcceptanceTester.Helpers.Db
{
    public static class SqlDbHelper
    {
        public static string GetDbName(Guid componentId)
        {
            return GetDbName(componentId, string.Empty);
        }

        public static string GetDbName(Guid componentId, string prefix)
        {
            if (prefix == null) throw new ArgumentNullException("prefix");
            if (componentId == Guid.Empty) throw new ArgumentException();
            return string.Format("ermeX_AcceptanceTest_{0}{1}", prefix, componentId.ToString().Replace('-', '_'));
        }


        public static string GetConnectionString(Guid componentId)
        {
           return GetConnectionString(GetDbName(componentId));
        }

        public static string GetConnectionString(Guid componentId, string prefix)
        {
            return GetConnectionString(GetDbName(componentId,prefix));
        }

        public static string GetConnectionString(string dbName)
        {
            return DbTestEnvironment.TestSqlServer.GetConnectionString(dbName);

        }

        public static string CreateDatabase(Guid componentId, string prefix)
        {
            string currentDbName = GetDbName(componentId,prefix);

            using (var conn = new SqlConnection(DbTestEnvironment.TestSqlServer.GetMasterConnectionString()))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = string.Format("SELECT count(1) FROM sys.databases WHERE name = N'{0}'", currentDbName);
                    conn.Open();
                    int result = Convert.ToInt32(cmd.ExecuteScalar());

                    if (result > 0)
                    {
                        DropDatabase(currentDbName, conn);
                    }

                    cmd.CommandText = string.Format("CREATE database {0}", currentDbName);
                    cmd.Dispose();

                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }

            SqlConnection.ClearAllPools();

            Thread.Sleep(1000);

            CreateLoggingTable(currentDbName);

            return currentDbName;
        }

        private static void CreateLoggingTable(string dbName)
        {
            using (var conn = new SqlConnection(GetConnectionString(dbName)))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "CREATE TABLE [dbo].[Log] ([Id] [int] IDENTITY (1, 1) NOT NULL,[Date] [datetime] NOT NULL,[Thread] [varchar] (255) NOT NULL,[Level] [varchar] (50) NOT NULL,[Logger] [varchar] (255) NOT NULL,[Message] [varchar] (4000) NOT NULL,[Exception] [varchar] (2000) NULL);";
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

       
        public static void DropDatabase(Guid componentId)
        {
            DropDatabase(componentId, string.Empty);
        }

        public static void DropDatabase(Guid componentId,string prefix)
        {
            var currentDbName = GetDbName(componentId,prefix);

            DropDatabase(currentDbName,null);
        }
        public static ISqlServerInfo DbInfo
        {
            get { return DbTestEnvironment.TestSqlServer; }
        }

        private static void DropDatabase(string currentDbName,SqlConnection masterConnection)
        {
            bool mustDispose = false;
            if (masterConnection == null)
            {
                masterConnection = new SqlConnection(DbTestEnvironment.TestSqlServer.GetMasterConnectionString());
                mustDispose = true;
                masterConnection.Open();
            }

            using (var cmd = masterConnection.CreateCommand())
            {
                cmd.CommandText =
                    string.Format("IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') drop database {0};",
                                  currentDbName);

                DisconnectAllUsersAndSetDbToSingleUserMode(masterConnection, currentDbName);
                Thread.Sleep(50);

                cmd.ExecuteNonQuery();
            }
            if (mustDispose)
            {
                masterConnection.Close();
                masterConnection.Dispose();
            }
        }

        private static void DisconnectAllUsersAndSetDbToSingleUserMode(SqlConnection masterConn , string currentDbName)
        {
            var sql = string.Format("IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') " +
                                    "ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", currentDbName);
            using (SqlCommand cmd = masterConn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }


        #region Nested type: DbTestEnvironment

        private static class DbTestEnvironment
        {
            private static volatile TestSqlServerInfo _testSqlServer;
            private static readonly object _locker = new object();

            public static TestSqlServerInfo TestSqlServer
            {
                get
                {
                    if (_testSqlServer == null)
                        lock (_locker)
                            if (_testSqlServer == null)
                            {
                                var server = Environment.GetEnvironmentVariable("TEST_SQL_SERVER_ADDRESS") ??
                                             "localhost";
                                var adminLogin =
                                    Environment.GetEnvironmentVariable("TEST_SQL_SERVER_ADMIN_LOGIN") ?? "sa";
                                var adminPassword =
                                    Environment.GetEnvironmentVariable("TEST_SQL_SERVER_ADMIN_PASSWORD") ??
                                    "sqlsql";
                                _testSqlServer = new TestSqlServerInfo(server, adminLogin, adminPassword);
                            }


                    return _testSqlServer;
                }
            }
        }

        #endregion

        #region Nested type: TestSqlServerInfo

       

        private class TestSqlServerInfo : ISqlServerInfo
        {
            public TestSqlServerInfo(string serverAddress, string adminLogin, string adminPassword)
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
                return string.Format("Data Source={0};Initial Catalog=Master;User ID={1};PWD={2};Enlist=false;Connection Timeout=60;",
                                     ServerAddress, AdminLogin, AdminPassword);
            }

            public string GetConnectionString(string dbName)
            {
                return string.Format("Data Source={0};User ID={1};PWD={2};Enlist=false;Initial Catalog={3};Connection Timeout=60;",
                                     ServerAddress, AdminLogin, AdminPassword, dbName);
            }
        }

        #endregion

        
    }
}
