﻿using System;

using NHibernate;
using ermeX.Logging;

namespace ermeX.DAL.Transactions
{
	internal class ErmexTransaction : IErmexTransaction
	{
		protected ILogger Logger { get; private set; }
		private readonly ITransaction _transaction;

		public ErmexTransaction(ITransaction transaction,ILogManager logManager)
		{
			Logger=logManager.GetLogger(GetType());
			Logger.Debug("cctor");
			if (transaction == null) throw new ArgumentNullException("transaction");
			_transaction = transaction;
		}

		public virtual void Commit()
		{
			Logger.Debug("Commit");
			_transaction.Commit();
		}

		public virtual void Rollback()
		{
			Logger.Warn("Rollback");
			_transaction.Rollback();
		}

		public void Dispose()
		{
			Logger.Debug("Dispose");
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			_transaction.Dispose();
		}
	}
}