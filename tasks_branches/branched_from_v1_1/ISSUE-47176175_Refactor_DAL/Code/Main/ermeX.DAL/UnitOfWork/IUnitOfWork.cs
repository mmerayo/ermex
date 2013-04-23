using System;
using NHibernate;

namespace ermeX.DAL.UnitOfWork
{
	internal interface IUnitOfWork : IDisposable
	{
		IUnitOfWorkFactory Factory { get; }
		ISession Session { get; }
		void Flush();
		void Commit();

		bool IsReadOnly { get; }
	}
}
