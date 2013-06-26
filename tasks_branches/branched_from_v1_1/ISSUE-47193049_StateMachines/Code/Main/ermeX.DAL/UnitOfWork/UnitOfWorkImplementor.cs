using System;
using System.Data;
using System.Threading;

using NHibernate;
using ermeX.DAL.Transactions;

namespace ermeX.DAL.UnitOfWork
{
	internal class UnitOfWorkImplementor : IUnitOfWork
	{
		protected readonly ILogger Logger = LogManager.GetLogger(typeof(UnitOfWorkImplementor).FullName);

		private readonly IUnitOfWorkFactory _factory;
		private readonly ISession _session;
		private readonly bool _autoCommitWhenDispose;
		private readonly IErmexTransaction _transaction;

		private readonly Guid _id = Guid.NewGuid();
		public UnitOfWorkImplementor(IUnitOfWorkFactory factory, ISession session, ITransactionProvider transactionProvider, bool autoCommitWhenDispose=false)
		{
			Logger.DebugFormat("cctor. Thread={0} - Id: {1}", Thread.CurrentThread.ManagedThreadId,_id);
			_factory = factory;
			_session = session;
			_autoCommitWhenDispose = autoCommitWhenDispose;
			_transaction = BeginTransaction(transactionProvider, IsolationLevel.ReadCommitted); //TODO: IsolationLevel.sERIALIZABLE THE SERIALIZABLE IS WHEN SEVERAL COMPONENTS SHARINGDB
		}
		~UnitOfWorkImplementor()
		{
			Dispose(false);
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			Logger.DebugFormat("Dispose. disposing={0} - Thread:{1} - Id: {2}", disposing, Thread.CurrentThread.ManagedThreadId, _id);
			if (disposing)
			{
				if (_autoCommitWhenDispose)
					Commit(true);
				else
				{
					if (_transaction != null)
						_transaction.Dispose();
				}
				_session.Connection.Close();
				_session.Dispose();
			}
			else
			{
				if(_autoCommitWhenDispose && IsInActiveTransaction)
				{
					Logger.FatalFormat("Despite UoW is configured to autocommit when disposed was never commited. - Id: {1}",_id);
				}
			}
		}

		private IErmexTransaction BeginTransaction(ITransactionProvider transactionProvider, IsolationLevel isolationLevel)
		{
			if (IsInActiveTransaction)
				return null;
			return transactionProvider.BeginTransaction(_session.BeginTransaction(isolationLevel));
		}

		public void Commit(bool disposing)
		{
			Logger.DebugFormat("Commit. disposing={0}, thread={1} - Id: {2}", disposing, Thread.CurrentThread.ManagedThreadId,_id);
			if (_autoCommitWhenDispose && !disposing)
				throw new InvalidOperationException(
					"The unit of work is configured to autocommit when dispose, Commit cannot be requested by caller as is automatic");

			if (_transaction == null)
				return;

			try
			{
				Flush();
				_transaction.Commit();
			}
			catch(Exception ex)
			{
				Logger.DebugFormat("RollingBack. Disposing={0}, thread={1} - Id: {2} - Exception:{3}", disposing, Thread.CurrentThread.ManagedThreadId, _id, ex.ToString());
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

		public bool IsReadOnly { get { return _session.FlushMode == FlushMode.Never; } }

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
			Logger.DebugFormat("Flush. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_session.Flush();
		}
	}
}