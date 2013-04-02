using System.Data;
using NHibernate;

namespace ermeX.DAL.DataAccess.UoW
{
	internal class UnitOfWorkImplementor : IUnitOfWork
	{
		private readonly IUnitOfWorkFactory _factory;
		private readonly ISession _session;

		public UnitOfWorkImplementor(IUnitOfWorkFactory factory, ISession session)
		{
			_factory = factory;
			_session = session;
		}

		public void Dispose()
		{
			_factory.DisposeUnitOfWork(this);
			_session.Dispose();
		}

		public IGenericTransaction BeginTransaction()
		{
			return new GenericTransaction(_session.BeginTransaction());
		}

		public IGenericTransaction BeginTransaction(IsolationLevel isolationLevel)
		{
			return new GenericTransaction(_session.BeginTransaction(isolationLevel));
		}

		public void TransactionalFlush()
		{
			TransactionalFlush(IsolationLevel.ReadCommitted);
		}

		public void TransactionalFlush(IsolationLevel isolationLevel)
		{
			IGenericTransaction tx = BeginTransaction(isolationLevel);
			try
			{
				//forces a flush of the current unit of work
				tx.Commit();
			}
			catch
			{
				tx.Rollback();
				throw;
			}
			finally
			{
				tx.Dispose();
			}
		}
	}
}