using System;

using NHibernate;

namespace ermeX.DAL.Transactions
{
	public class ErmexTransaction : IErmexTransaction
	{
		private static readonly ILogger Logger = LogManager.GetLogger(typeof(ErmexTransaction).FullName);
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

		public void Dispose()
		{
			Logger.DebugFormat("Dispose thread={0} ", System.Threading.Thread.CurrentThread.ManagedThreadId);
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			_transaction.Dispose();
		}
	}
}