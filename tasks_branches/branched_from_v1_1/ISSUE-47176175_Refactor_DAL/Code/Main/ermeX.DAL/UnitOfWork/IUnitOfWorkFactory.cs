using System;
using ermeX.DAL.Providers;
using ermeX.DAL.Transactions;

namespace ermeX.DAL.UnitOfWork
{
	internal interface IUnitOfWorkFactory
	{
		IUnitOfWork Create(bool readOnly, bool autoCommitWhenDispose=false);

		TResult ExecuteInUnitOfWork<TResult>(bool readOnly, Func<IUnitOfWork, TResult> atomicFunction);
		void ExecuteInUnitOfWork(bool readOnly, Action<IUnitOfWork> atomicAction);
		 
	}
}