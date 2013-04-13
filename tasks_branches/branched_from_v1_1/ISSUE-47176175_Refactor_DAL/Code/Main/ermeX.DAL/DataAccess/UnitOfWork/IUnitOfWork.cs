using System;
using NHibernate;

namespace ermeX.DAL.DataAccess.UnitOfWork
{
	internal interface IUnitOfWork : IDisposable
	{
		IUnitOfWorkFactory Factory { get; }
		ISession Session { get; }
		void Flush();
		void Commit();
	}
}
