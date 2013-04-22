using System;

namespace ermeX.DAL.UnitOfWork
{
	internal interface IUnitOfWorkFactory
	{
		IUnitOfWork Create(bool autoCommitWhenDispose=false);

		TResult ExecuteInUnitOfWork<TResult>(Func<IUnitOfWork,TResult> atomicFunction);
		void ExecuteInUnitOfWork(Action<IUnitOfWork> atomicAction);
	}
}