using System;
using System.Threading;
using Common.Logging;
using NHibernate;

namespace ermeX.DAL.Transactions
{
	//justified until this is investigated http://www.sqlite.org/cvstrac/wiki?p=MultiThreading
	//used toghether with system.data.sqlite
	public sealed class MutexedTransaction : ErmexTransaction
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(MutexedTransaction).FullName);
		private Mutex _mutex;

		public MutexedTransaction(string namedMutexName, ITransaction transaction):base(transaction)
		{
			_mutex = new Mutex(false, namedMutexName);
			Logger.DebugFormat("cctor. Waiting for mutex={2}. AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId, namedMutexName);
			_mutex.WaitOne();
			Logger.DebugFormat("cctor. Start critical section mutex={2}. AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId,namedMutexName);
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