using System;
using Common.Logging;
using NHibernate;
using ermeX.DAL.DataAccess.Repository;

namespace ermeX.DAL.DataAccess.UnitOfWork
{
	public class GenericTransaction : IGenericTransaction
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(GenericTransaction).FullName);
		private readonly ITransaction _transaction;

		public GenericTransaction(ITransaction transaction)
		{
			Logger.Debug("cctor");
			if (transaction == null) throw new ArgumentNullException("transaction");
			_transaction = transaction;
		}

		public void Commit()
		{
			Logger.Debug("Commit");
			_transaction.Commit();
		}

		public void Rollback()
		{
			Logger.Debug("Rollback");
			_transaction.Rollback();
		}

		public void Dispose()
		{
			Logger.Debug("Dispose");
			_transaction.Dispose();
		}
	}
}