using System;
using System.Threading;

using NHibernate;
using ermeX.Logging;

namespace ermeX.DAL.Transactions
{
	//justified until this is investigated http://www.sqlite.org/cvstrac/wiki?p=MultiThreading
	//used toghether with system.data.sqlite
	internal sealed class MutexedTransaction : ErmexTransaction
	{
		private Mutex _mutex;
		private static readonly TimeSpan WaitForMutexTimeSpan= TimeSpan.FromSeconds(10);

		public MutexedTransaction(string namedMutexName, ITransaction transaction)
			:base(transaction)
		{
			_mutex = new Mutex(false, namedMutexName);
			Logger.Debug(string.Format("cctor. Waiting for mutex={0}.", namedMutexName));
			if(!_mutex.WaitOne(WaitForMutexTimeSpan))
				throw new TransactionException("Could not obtain mutex on time"); //TODO: THROW custom transaction
			Logger.Debug(string.Format("cctor. Start critical section mutex={0}.", namedMutexName));
		}

		public override void Commit()
		{
			base.Commit();
			_mutex.ReleaseMutex();
			Logger.DebugFormat("Commit. released mutex. AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
		}

		public override void Rollback()
		{
			base.Rollback();
			_mutex.ReleaseMutex();
			Logger.DebugFormat("Rollback. released mutex. AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			//_mutex.ReleaseMutex();
			//Logger.DebugFormat("Dispose. released mutex. AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
			_mutex.Dispose();
			Logger.DebugFormat("Dispose. Disposed mutex. AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
		}
	}
}