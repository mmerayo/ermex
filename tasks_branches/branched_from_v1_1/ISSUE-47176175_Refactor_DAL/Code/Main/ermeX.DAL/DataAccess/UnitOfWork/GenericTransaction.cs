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
			Logger.DebugFormat("cctor thread {0} ",System.Threading.Thread.CurrentThread.ManagedThreadId);
			if (transaction == null) throw new ArgumentNullException("transaction");
			_transaction = transaction;
		}

		public void Commit()
		{
			Logger.DebugFormat("Commit thread={0} ", System.Threading.Thread.CurrentThread.ManagedThreadId);
			_transaction.Commit();
		}

		public void Rollback()
		{
			Logger.DebugFormat("Rollback thread={0} ", System.Threading.Thread.CurrentThread.ManagedThreadId);
			_transaction.Rollback();
		}

		public void Dispose()
		{
			Logger.DebugFormat("Dispose thread={0} ", System.Threading.Thread.CurrentThread.ManagedThreadId);
			_transaction.Dispose();
		}
	}
}