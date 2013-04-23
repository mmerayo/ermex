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
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ErmexTransaction).FullName);
		private readonly string _namedMutexName;
		private readonly ITransaction _transaction;
		private readonly Mutex _mutex;

		public MutexedTransaction(string namedMutexName, ITransaction transaction):base(transaction)
		{
			_namedMutexName = namedMutexName;
			_mutex = new Mutex(false, namedMutexName);
			_mutex.WaitOne();
		}

		public override void Commit()
		{
			base.Commit();
			_mutex.ReleaseMutex();
		}

		public override void Rollback()
		{
			base.Rollback();
			_mutex.ReleaseMutex();
		}

		public override void Dispose()
		{
			base.Dispose();
			_mutex.Dispose();
		}
	}
}