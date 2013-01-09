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
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using NHibernate;
using NHibernate.Exceptions;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.Providers;
using ermeX.DAL.Interfaces;

namespace ermeX.DAL.DataAccess.Helpers
{
    class DataAccessExecutor : IDataAccessExecutor
    {
        public IDalSettings DalSettings { get; private set; }
        public ISessionProvider SessionProvider { get; private set; }

        [Inject]
        public DataAccessExecutor(IDalSettings dalSettings)
        {
            DalSettings = dalSettings;
            if (dalSettings == null) throw new ArgumentNullException("dalSettings");
            SessionProvider = new SessionProvider(dalSettings);
            LoadRetryStrategy(dalSettings.ConfigurationSourceType);
        }

        private IStorageOperationRetryStrategy RetryStrategy { get; set; }

        private void LoadRetryStrategy(DbEngineType engineType)
        {
            const int retriesDeadlock = 15;//TODO: TO DAL SETTINGS
            const int retriesTimeout = 3;
            switch (engineType)
            {
                case DbEngineType.SqlServer2008:
                    RetryStrategy=new SqlServerRetryPolicy(retriesDeadlock,retriesTimeout); 
                    break;
                case DbEngineType.Sqlite:
                case DbEngineType.SqliteInMemory:
                    RetryStrategy = new SqliteRetryPolicy(retriesDeadlock, retriesTimeout);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("engineType");
            }
        }

        public DataAccessOperationResult<TResult> Perform<TResult>(Func<ISession, DataAccessOperationResult<TResult>> innerOperation)
        {
            return Perform(new[] {innerOperation},innerOperation);
        }

        public DataAccessOperationResult<TResult> Perform<TResult>(IEnumerable<Func<ISession, DataAccessOperationResult<TResult>>> innerChainOfOperations,
            Func<ISession, DataAccessOperationResult<TResult>> operationExtractsResult=null)
        {
            const int millisecondsRetry = 500;//TODO: configuration

            if (innerChainOfOperations == null) throw new ArgumentNullException("innerChainOfOperations");
            if(operationExtractsResult!=null && !innerChainOfOperations.Contains(operationExtractsResult))
                throw new InvalidOperationException("The operation that extracts the result must be contained in the chain of operations");

            var result = new DataAccessOperationResult<TResult>();

            while (true)
            {
                using (var session = SessionProvider.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var innerOperation in innerChainOfOperations)
                        {
                            var operationResult = innerOperation(session);
                            if (!operationResult.Success)
                            {
                                transaction.Rollback();
                                result.ResultValue = default(TResult);
                                return result;
                            }
                            if (innerOperation == operationExtractsResult)
                                result.ResultValue = operationResult.ResultValue;
                        }
                        //session.Flush();
                        transaction.Commit();
                        result.Success = true;
                        break;
                    }
                    catch (StaleStateException ex)
                    {
                        throw new InvalidOperationException("Attempt to update an unexising row", ex);
                    }
                    catch (ADOException e)
                    {
                        try
                        {
                            if (!transaction.WasRolledBack)
                                transaction.Rollback();
                        }catch(TransactionException ex)
                        {
                            //TODO: LOG THIS ONE
                        }
                        var dbException = ADOExceptionHelper.ExtractDbException(e);

                        if (RetryStrategy.Retry(dbException))
                        {
                            
                            Thread.Sleep(millisecondsRetry);
                            continue;
                        }

                        //TODO: log dbexception AS WARNING

                        throw;
                    }
                }
            }

            return result;
        }
        
        private interface IStorageOperationRetryStrategy
        {
            bool Retry(DbException dbException);
        }

        private class SqlServerRetryPolicy : IStorageOperationRetryStrategy
        {
            private const int SqlDeadLockErrorNumber = 1205;
            private const int SqlTimeOutErrorNumber = -2;
            private int _triesDeadlock;
            private int _triesTimeout;

            public SqlServerRetryPolicy(int triesDeadlock,int triesTimeout=1)
            {
                if (triesDeadlock < 1) throw new ArgumentOutOfRangeException("triesDeadlock");
                if (triesTimeout < 1) throw new ArgumentOutOfRangeException("triesTimeout");
                _triesDeadlock = triesDeadlock;
                _triesTimeout = triesTimeout;
            }

            public bool Retry(DbException dbException)
            {
                var sqlException = dbException as SqlException;
                if (sqlException == null) 
                    throw new ArgumentException("dbException must be a not null SqlException");
                
                return NeedRetry(sqlException);
            }

            private bool NeedRetry(SqlException ex)
            {
                if (ex.Number == SqlDeadLockErrorNumber)
                    return --_triesDeadlock > 0;
                if (ex.Number == SqlTimeOutErrorNumber)
                    return --_triesTimeout > 0;
                return false;
            }

           
        }

        private class SqliteRetryPolicy : IStorageOperationRetryStrategy
        {
            
            private int _triesDeadlock;
            private int _triesTimeout;

            public SqliteRetryPolicy(int triesDeadlock, int triesTimeout = 1)
            {
                if (triesDeadlock < 1) throw new ArgumentOutOfRangeException("triesDeadlock");
                if (triesTimeout < 1) throw new ArgumentOutOfRangeException("triesTimeout");
                _triesDeadlock = triesDeadlock;
                _triesTimeout = triesTimeout;
            }

            public bool Retry(DbException dbException)
            {
                return false;
            }

        }
    }

    
}
