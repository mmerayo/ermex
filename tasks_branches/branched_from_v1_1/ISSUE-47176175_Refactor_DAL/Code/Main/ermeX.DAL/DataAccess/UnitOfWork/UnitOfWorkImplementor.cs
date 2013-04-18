using System;
using System.Data;
using Common.Logging;
using NHibernate;
using ermeX.ConfigurationManagement.Settings;

namespace ermeX.DAL.DataAccess.UnitOfWork
{
	internal class UnitOfWorkImplementor : IUnitOfWork
	{
		protected readonly ILog Logger = LogManager.GetLogger(typeof(UnitOfWorkImplementor).FullName);

		private readonly IUnitOfWorkFactory _factory;
		private readonly ISession _session;
		private readonly bool _autoCommitWhenDispose;
		private readonly IGenericTransaction _transaction;

		public UnitOfWorkImplementor(IUnitOfWorkFactory factory, ISession session, bool autoCommitWhenDispose=false)
		{
			Logger.Debug("cctor");
			_factory = factory;
			_session = session;
			_autoCommitWhenDispose = autoCommitWhenDispose;
			_transaction = BeginTransaction(IsolationLevel.ReadCommitted);
		}
		~UnitOfWorkImplementor()
		{
			Dispose(false);
		}
		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			Logger.DebugFormat("Dispose. disposing={0}",disposing);
			if (disposing)
			{
				if (_autoCommitWhenDispose)
					Commit(true);
				else
				{
					if (_transaction != null)
						_transaction.Dispose();
				}
				_session.Dispose();
			}
			else
			{
				if(_autoCommitWhenDispose && IsInActiveTransaction)
				{
					Logger.Fatal(m=>m("despite UoW is configured to autocommit when disposed was never commited."));
				}
			}
		}

		private IGenericTransaction BeginTransaction(IsolationLevel isolationLevel)
		{
			if (IsInActiveTransaction)
				return null;
			return new GenericTransaction(_session.BeginTransaction(isolationLevel));
		}

		public void Commit(bool disposing)
		{
			Logger.DebugFormat("Commit. disposing={0}", disposing);
			if (_autoCommitWhenDispose && !disposing)
				throw new InvalidOperationException(
					"The unit of work is configured to autocommit when dispose, Commit cannot be requested by caller as is automatic");

			if (_transaction == null)
				return;

			try
			{
				_transaction.Commit();
				Flush();
			}
			catch
			{
				_transaction.Rollback();
				throw;
			}
			finally
			{
				_transaction.Dispose();
			}
		}

		public void Commit()
		{
			Commit(false);
		}

		private bool IsInActiveTransaction
		{
			get { return _session.Transaction!=null && _session.Transaction.IsActive; }
		}

		public IUnitOfWorkFactory Factory
		{
			get { return _factory; }
		}

		public ISession Session
		{
			get { return _session; }
		}

		public void Flush()
		{
			Logger.Debug("Flush");
			_session.Flush();
		}
	}
}