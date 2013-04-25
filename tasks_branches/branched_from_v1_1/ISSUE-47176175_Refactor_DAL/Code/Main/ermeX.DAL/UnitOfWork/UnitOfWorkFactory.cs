using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Threading;
using Common.Logging;
using NHibernate;
using NHibernate.Exceptions;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.Providers;
using ermeX.DAL.Transactions;

namespace ermeX.DAL.UnitOfWork
{
	internal class UnitOfWorkFactory : IUnitOfWorkFactory
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (UnitOfWorkFactory).FullName);
		private ISessionProvider _sessionFactory;
		private IStorageOperationRetryStrategy _retryStrategy;

		[Inject]
		public UnitOfWorkFactory(ISessionProvider sessionProvider, IDalSettings settings,
			IReadTransactionProvider readTransactionsProvider,IWriteTransactionProvider writeTransactionProvider)
		{
			Logger.DebugFormat("cctor: thread={0}", Thread.CurrentThread.ManagedThreadId);
			
			ReadOnlyTransactionsProvider = readTransactionsProvider;
			WrittableTransactionsProvider = writeTransactionProvider;
			_sessionFactory = sessionProvider;

			SetRetryStrategy(settings);
		}

		private void SetRetryStrategy(IDalSettings settings)
		{
			if (settings == null)
				return;

			const int retriesDeadlock = 15; //TODO: TO DAL SETTINGS with default value
			const int retriesTimeout = 15;
			switch (settings.ConfigurationSourceType)
			{
				case DbEngineType.SqlServer2008:
					_retryStrategy = new SqlServerRetryPolicy(retriesDeadlock, retriesTimeout);
					break;
				case DbEngineType.Sqlite:
				case DbEngineType.SqliteInMemory:
					_retryStrategy = new SqliteRetryPolicy(retriesDeadlock, retriesTimeout);
					break;
				default:
					throw new NotSupportedException(settings.ConfigurationSourceType.ToString());
			}
		}

		public IReadTransactionProvider ReadOnlyTransactionsProvider { get; private set; }
		public IWriteTransactionProvider WrittableTransactionsProvider { get; private set; }


		public IUnitOfWork Create(bool readOnly, bool autoCommitWhenDispose = false)
		{
			Logger.DebugFormat("Create. autoCommitWhenDispose={0} Thread={1}", autoCommitWhenDispose,
			                   Thread.CurrentThread.ManagedThreadId);
			var session = _sessionFactory.OpenSession(readOnly);
			var transactionProvider = (readOnly ? (ITransactionProvider)ReadOnlyTransactionsProvider : (ITransactionProvider)WrittableTransactionsProvider);
			return new UnitOfWorkImplementor(this, session, transactionProvider, autoCommitWhenDispose);
		}

		public void ExecuteInUnitOfWork(bool readOnly, Action<IUnitOfWork> atomicAction)
		{
			ExecuteInUnitOfWork(readOnly, uow =>
				{
					atomicAction(uow);
					return new object();
				});
		}

		public TResult ExecuteInUnitOfWork<TResult>(bool readOnly, Func<IUnitOfWork, TResult> atomicFunction)
		{
			Logger.Debug("ExecuteInUnitOfWork - Start");

			const int millisecondsRetry = 250; //TODO: TO DAL SETTINGS with default value

			TResult result;
			while (true)
			{
				try
				{
					using (var uow = Create(readOnly))
					{
						result = atomicFunction(uow);
						uow.Commit();
					}
					return result;
				}
				catch (ADOException e)
				{

					var dbException = ADOExceptionHelper.ExtractDbException(e);

					if (_retryStrategy != null && _retryStrategy.Retry(dbException))
					{
						Logger.DebugFormat("ExecuteInUnitOfWork: Retrying - AppDomain: {0} -Thread: {1}", AppDomain.CurrentDomain.Id,
						                   Thread.CurrentThread.ManagedThreadId);
						Thread.Sleep(millisecondsRetry);
						continue;
					}

					Logger.Error("ExecuteInUnitOfWork: wont be retried");

					throw;
				}
				catch (StaleStateException ex)
				{
					Logger.ErrorFormat("ExecuteInUnitOfWork: AppDomain: {0} - Thread: {1} - Exception: {2}", AppDomain.CurrentDomain.Id,
										   Thread.CurrentThread.ManagedThreadId, ex.ToString());
					if (_retryStrategy != null && _retryStrategy.Retry(ex))
					{
						Logger.DebugFormat("ExecuteInUnitOfWork: Retrying - AppDomain: {0} -Thread: {1}", AppDomain.CurrentDomain.Id,
										   Thread.CurrentThread.ManagedThreadId);
						Thread.Sleep(millisecondsRetry);
						continue;
					}
					Logger.Error("ExecuteInUnitOfWork: wont be retried");
					throw;
				}
				catch (TransactionException ex)
				{
					Logger.ErrorFormat("ExecuteInUnitOfWork: AppDomain: {0} - Thread: {1} - Exception: {2}", AppDomain.CurrentDomain.Id,
						                   Thread.CurrentThread.ManagedThreadId,ex.ToString());
					
					//TODO: HANDLE OR REMOVE
					if (_retryStrategy != null && _retryStrategy.Retry(ex))
					{
						Logger.DebugFormat("ExecuteInUnitOfWork: Retrying - AppDomain: {0} -Thread: {1}", AppDomain.CurrentDomain.Id,
										   Thread.CurrentThread.ManagedThreadId);
						Thread.Sleep(millisecondsRetry * 4);
						continue;
					}
					Logger.Error("ExecuteInUnitOfWork: wont be retried");
					throw;

				}
				catch (Exception ex)
				{
					Logger.ErrorFormat("ExecuteInUnitOfWork: {0}", ex.ToString());
					throw;
				}

			}
		}

		private interface IStorageOperationRetryStrategy
		{
			bool Retry(DbException dbException);
			bool Retry(TransactionException dbException);
			bool Retry(StaleStateException dbException);
		}

		private class SqlServerRetryPolicy : IStorageOperationRetryStrategy
		{
			private const int SqlDeadLockErrorNumber = 1205;
			private const int SqlTimeOutErrorNumber = -2;
			private int _triesDeadlock;
			private int _triesTimeout;

			public SqlServerRetryPolicy(int triesDeadlock, int triesTimeout = 1)
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

			public bool Retry(TransactionException dbException)
			{
				return --_triesDeadlock > 0;
			}

			public bool Retry(StaleStateException dbException)
			{
				return --_triesDeadlock > 0;
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
				//this does not work --> var sqlException = dbException as SQLiteException;
				//if (sqlException == null)
				//    throw new ArgumentException(string.Format("dbException must be a not null SQLiteException and is {0}",
				//                                              dbException == null ? "null" : dbException.GetType().FullName));

				if (!dbException.GetType().Namespace.ToLowerInvariant().StartsWith("system.data.sqlite"))
					throw dbException;
				
				return NeedRetry(dbException);
			}

			public bool Retry(TransactionException dbException)
			{
				return --_triesDeadlock > 0;
			}

			public bool Retry(StaleStateException dbException)
			{
				return --_triesDeadlock > 0;
			}

			private bool NeedRetry(DbException ex)
			{
				switch ((SQLiteErrorCode)ex.ErrorCode)
				{
					case SQLiteErrorCode.Busy:
						return --_triesTimeout > 0;
					case SQLiteErrorCode.Locked:
						return --_triesDeadlock > 0;
					default:
						return false;
				}
			}

		}
	}

}