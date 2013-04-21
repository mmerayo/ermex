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

namespace ermeX.DAL.UnitOfWork
{
	internal class UnitOfWorkFactory : IUnitOfWorkFactory
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(UnitOfWorkFactory).FullName);
		private readonly ISessionProvider _sessionFactory;
		private readonly IStorageOperationRetryStrategy _retryStrategy;

		[Inject]
		public UnitOfWorkFactory(ISessionProvider sessionProvider,IDalSettings settings)
		{
			Logger.DebugFormat("cctor: thread={0}",Thread.CurrentThread.ManagedThreadId);
			_sessionFactory = sessionProvider;

			const int retriesDeadlock = 15;//TODO: TO DAL SETTINGS
			const int retriesTimeout = 3;
			switch(settings.ConfigurationSourceType)
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

		public IUnitOfWork Create (bool autoCommitWhenDispose=false)
		{
			Logger.DebugFormat("Create. autoCommitWhenDispose={0} Thread={1}",autoCommitWhenDispose,Thread.CurrentThread.ManagedThreadId);
			var session = _sessionFactory.OpenSession();
			session.FlushMode = FlushMode.Commit;
			return new UnitOfWorkImplementor(this, session,autoCommitWhenDispose);
		}

		public void ExecuteInUnitOfWork(Action atomicAction)
		{
			ExecuteInUnitOfWork(() =>
			                    	{
			                    		atomicAction();
			                    		return new object();
			                    	});
		}

		public TResult ExecuteInUnitOfWork<TResult>(Func<TResult> atomicFunction)
		{
			const int millisecondsRetry = 500;

			TResult result;
			while (true)
			{
				using (var uow = Create())
				{
					try
					{
						result=atomicFunction();
						uow.Commit();
						return result;
					}
					catch (ADOException e)
					{
						try
						{
							if (!uow.Session.Transaction.WasRolledBack)
								uow.Session.Transaction.Rollback();
						}
						catch (TransactionException ex)
						{
							Logger.WarnFormat("ExecuteInUnitOfWork. {0}",ex);
						}
						var dbException = ADOExceptionHelper.ExtractDbException(e);

						if (_retryStrategy.Retry(dbException))
						{
							Thread.Sleep(millisecondsRetry);
							continue;
						}

						Logger.Error("ExecuteInUnitOfWork: wont be retried");

						throw;
					}
				}
			}
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
				var sqlException = dbException as SQLiteException;
				if (sqlException == null)
					throw new ArgumentException("dbException must be a not null SqlException");

				return NeedRetry(sqlException);
			}

			private bool NeedRetry(SQLiteException ex)
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