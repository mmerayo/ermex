using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NHibernate;

namespace ermeX.DAL.DataAccess.UoW
{
	internal interface IUnitOfWork : IDisposable
	{
		IGenericTransaction BeginTransaction();
		IGenericTransaction BeginTransaction(IsolationLevel isolationLevel);
		void TransactionalFlush();
		void TransactionalFlush(IsolationLevel isolationLevel);
		bool IsInActiveTransaction { get; }
		IUnitOfWorkFactory Factory { get; }
		ISession Session { get; }
		void Flush();
	}
}
