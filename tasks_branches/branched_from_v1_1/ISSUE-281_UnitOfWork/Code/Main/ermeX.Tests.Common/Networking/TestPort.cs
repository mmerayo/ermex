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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.Domain.Implementations.QueryDatabase;
using ermeX.Domain.QueryDatabase;
using ermeX.NonMerged;
using ermeX.Tests.Common.DataAccess;
using Network = ermeX.Common.Networking;
namespace ermeX.Tests.Common.Networking
{
    public sealed class TestPort : IDisposable
    {
        private const string DbName = "PortBooking";
        private const string LockDbCreationMutexName = "ermeX.Tests.Common.Networking.SharedPorts";

        private const string CreateTableQuery =
            "CREATE TABLE IF NOT EXISTS PortsBooked (PortNumber INT PRIMARY KEY ASC UNIQUE, CreatedDateUtc INTEGER NOT NULL );";

        private static readonly SqliteDbEngine SqliteDbEngine;
        private static readonly IQueryHelper QueryHelper;

        static TestPort()
        {
            ResolveUnmerged.Init();
            using (var mutex = new Mutex(false, LockDbCreationMutexName))
            {
                mutex.WaitOne(TimeSpan.FromSeconds(10));

                SqliteDbEngine = new SqliteDbEngine(DbName, false, Environment.GetEnvironmentVariable("TEST_PORTS_DB_PATH") ?? "c:\\");
                    //TODO: get THE db FOLDER from A CONFIG FILE, it must be shared between al builds
                SqliteDbEngine.CreateDatabase();
                QueryHelper = new QueryHelperFactory().GetHelper(DbEngineType.SqliteInMemory,
                                                    SqliteDbEngine.GetConnectionString());
                QueryHelper.ExecuteNonQuery(
                    CreateTableQuery);
            }
        }


        private TestPort(ushort bottomRange, ushort topRange)
        {
            BookPort(bottomRange, topRange);
        }

        public TestPort(ushort bottomRange) : this(bottomRange, ushort.MaxValue)
        {
        }

        private void BookPort(ushort bottomRange, ushort topRange)
        {
            bool booked = false;

            ushort candidatePort = 0;
            do
            {
                try
                {
                    candidatePort = Network.GetFreePort(bottomRange, topRange);

                    QueryHelper.ExecuteNonQuery(
                        string.Format("INSERT INTO PortsBooked (PortNumber,CreatedDateUtc) VALUES ({0}, {1})",
                                      candidatePort, DateTime.UtcNow.Ticks));
                    booked = true;
                }
                catch (SQLiteException ex)
                {
                    switch ((SQLiteErrorCode) ex.ErrorCode)
                    {
                        case SQLiteErrorCode.Constraint:
                            bottomRange = (ushort) (candidatePort + 1);
                            break;
                        case SQLiteErrorCode.Error:
                        case SQLiteErrorCode.Busy:
                        case SQLiteErrorCode.Locked:   
                            break;

                        default:
                            throw new Exception(string.Format("SqliteErrorCode {0}", ex.ErrorCode.ToString(CultureInfo.InvariantCulture)),ex);
                    }
                }
            } while (!booked);
            PortNumber = candidatePort;
        }

        public ushort PortNumber { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                //managed
            }
            //unmanaged

            bool mustRetry=false;
            do
            {
                try
                {
                    QueryHelper.ExecuteNonQuery(string.Format("DELETE FROM PortsBooked WHERE CreatedDateUtc<{0}",
                                                              DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(10)).Ticks));
                }
                catch (SQLiteException ex)
                {
                    switch ((SQLiteErrorCode) ex.ErrorCode)
                    {

                        case SQLiteErrorCode.Error: //already deleted
                            break;
                        case SQLiteErrorCode.Busy:
                        case SQLiteErrorCode.Locked:
                            mustRetry = true;
                            break;
                        default:
                            throw ex;
                    }
                }
            } while (mustRetry);
        }

        ~TestPort()
        {
            try
            {
                Dispose(false);
            }catch{}
        }

        public static implicit operator ushort(TestPort port)
        {
            return port.PortNumber;
        }

        public static implicit operator int(TestPort port)
        {
            return port.PortNumber;
        }

        public override string ToString()
        {
            return ((ushort)PortNumber).ToString();
        }
    }
}
