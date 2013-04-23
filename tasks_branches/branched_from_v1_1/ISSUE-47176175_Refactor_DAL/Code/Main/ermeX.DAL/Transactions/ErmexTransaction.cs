using System;
using Common.Logging;
using NHibernate;

namespace ermeX.DAL.Transactions
{
	public class ErmexTransaction : IErmexTransaction
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ErmexTransaction).FullName);
		private readonly ITransaction _transaction;

		public ErmexTransaction(ITransaction transaction)
		{
			Logger.DebugFormat("cctor thread {0} ",System.Threading.Thread.CurrentThread.ManagedThreadId);
			if (transaction == null) throw new ArgumentNullException("transaction");
			_transaction = transaction;
		}

		public virtual void Commit()
		{
			Logger.DebugFormat("Commit thread={0} ", System.Threading.Thread.CurrentThread.ManagedThreadId);
			_transaction.Commit();
		}

		public virtual void Rollback()
		{
			Logger.WarnFormat("Rollback thread={0} ", System.Threading.Thread.CurrentThread.ManagedThreadId);
			_transaction.Rollback();
		}

		public virtual void Dispose()
		{
			Logger.DebugFormat("Dispose thread={0} ", System.Threading.Thread.CurrentThread.ManagedThreadId);
			_transaction.Dispose();
		}
	}
}