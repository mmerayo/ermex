using System;
using NHibernate;

namespace ermeX.DAL.DataAccess.UnitOfWork
{
	public class GenericTransaction : IGenericTransaction
	{
		private readonly ITransaction _transaction;

		public GenericTransaction(ITransaction transaction)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			_transaction = transaction;
		}

		public void Commit()
		{
			_transaction.Commit();
		}

		public void Rollback()
		{
			_transaction.Rollback();
		}

		public void Dispose()
		{
			_transaction.Dispose();
		}
	}
}