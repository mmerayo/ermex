using System;
using System.Data;
using NHibernate;

namespace ermeX.DAL.DataAccess.UoW
{
	internal class UnitOfWorkImplementor : IUnitOfWork
	{
		private readonly IUnitOfWorkFactory _factory;
		private readonly ISession _session;
		private readonly IGenericTransaction _transaction;

		public UnitOfWorkImplementor(IUnitOfWorkFactory factory, ISession session)
		{
			_factory = factory;
			_session = session;
			_transaction = BeginTransaction(IsolationLevel.ReadCommitted);
		}

		public void Dispose()
		{
			_factory.DisposeUnitOfWork(this);
			if (_transaction != null)
				_transaction.Dispose();
			_session.Dispose();
		}

		private IGenericTransaction BeginTransaction(IsolationLevel isolationLevel)
		{
			if (IsInActiveTransaction)
				return null;
			return new GenericTransaction(_session.BeginTransaction(isolationLevel));
		}

		public void Commit()
		{
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
			_session.Flush();
		}
	}
}