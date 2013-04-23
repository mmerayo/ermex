using System;
using ermeX.DAL.Providers;
using ermeX.DAL.Transactions;

namespace ermeX.DAL.UnitOfWork
{
	internal interface IUnitOfWorkFactory
	{
		IUnitOfWork Create(bool autoCommitWhenDispose=false);

		TResult ExecuteInUnitOfWork<TResult>(Func<IUnitOfWork,TResult> atomicFunction);
		void ExecuteInUnitOfWork(Action<IUnitOfWork> atomicAction);
		ITransactionProvider TransactionsProvider { get; }
	}
}