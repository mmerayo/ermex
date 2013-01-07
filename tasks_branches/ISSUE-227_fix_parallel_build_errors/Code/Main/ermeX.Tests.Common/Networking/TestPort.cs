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
using System.Linq;
using System.Text;
using System.Threading;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.Tests.Common.DataAccess;
using Network = ermeX.Common.Networking;
namespace ermeX.Tests.Common.Networking
{
    internal sealed class TestPort:IDisposable
    {
        private const string DbName = "PortBooking";
        private const string LockDbCreationMutexName = "ermeX.Tests.Common.Networking.SharedPorts";
        private static readonly SqliteDbEngine SqliteDbEngine;
        private static readonly QueryHelper QueryHelper;

        static TestPort()
        {
            using (var mutex = new Mutex(false, LockDbCreationMutexName))
            {
                mutex.WaitOne(TimeSpan.FromSeconds(10));

                SqliteDbEngine = new SqliteDbEngine(DbName,false,"d:\\");//TODO: get THE db FOLDER from A CONFIG FILE, it must be shared between al builds
                SqliteDbEngine.CreateDatabase();
                QueryHelper = QueryHelper.GetHelper(DbEngineType.SqliteInMemory,
                                                    SqliteDbEngine.GetConnectionString());
                QueryHelper.ExecuteNonQuery(
                    "CREATE TABLE IF NOT EXISTS PortsBooked (PortNumber INT PRIMARY KEY ASC UNIQUE );");
            }
        }


        public TestPort(ushort bottomRange, ushort topRange)
        {
            BookPort(bottomRange, topRange);
        }

        public TestPort(ushort bottomRange):this(bottomRange,ushort.MaxValue){}

        private void BookPort(ushort bottomRange, ushort topRange)
        {
            bool booked = false;

            ushort candidatePort=0;
            do
            {
                try
                {
                    using (var mutex = new Mutex(false, LockDbCreationMutexName))
                    {
                        candidatePort = Network.GetFreePort(bottomRange, topRange);

                        mutex.WaitOne(TimeSpan.FromSeconds(10));
                        QueryHelper.ExecuteNonQuery(string.Format("INSERT INTO PortsBooked (PortNumber) VALUES ({0})",
                                                                  candidatePort));
                    }
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
                            using (var mutex = new Mutex(false, LockDbCreationMutexName))
                            {
                                mutex.WaitOne(TimeSpan.FromSeconds(10));
                                SqliteDbEngine.CreateDatabase();
                                QueryHelper.ExecuteNonQuery(
                                    "CREATE TABLE IF NOT EXISTS PortsBooked (PortNumber INT PRIMARY KEY ASC UNIQUE );");
                            }
                            break;

                        default:
                            throw ex;
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


            using (var mutex = new Mutex(false, LockDbCreationMutexName))
            {
                mutex.WaitOne(TimeSpan.FromSeconds(10));
                try
                {
                    QueryHelper.ExecuteNonQuery(string.Format("DELETE FROM PortsBooked WHERE PortNumber={0}", PortNumber));

                    var items =
                        QueryHelper.ExecuteScalar<int>(
                            string.Format("SELECT COUNT(*) FROM PortsBooked WHERE PortNumber={0}", PortNumber));
                    if (items == 0)
                        SqliteDbEngine.DropDatabase();
                }
                catch (SQLiteException ex)
                {
                    switch ((SQLiteErrorCode)ex.ErrorCode)
                    {

                        case SQLiteErrorCode.Error://already deleted
                            break;
                        default:
                            throw ex;
                    }
                }
            }
        }

        ~TestPort()
        {
            Dispose(false);
        }

        public static implicit operator ushort(TestPort port)
        {
            return port.PortNumber;
        }

        public static implicit operator int(TestPort port)
        {
            return port.PortNumber;
        }

    }
}
