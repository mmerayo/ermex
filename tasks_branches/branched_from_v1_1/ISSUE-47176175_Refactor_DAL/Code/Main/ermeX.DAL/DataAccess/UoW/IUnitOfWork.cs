using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ermeX.DAL.DataAccess.UoW
{
	internal interface IUnitOfWork : IDisposable
	{
		IGenericTransaction BeginTransaction();
		IGenericTransaction BeginTransaction(IsolationLevel isolationLevel);
		void TransactionalFlush();
		void TransactionalFlush(IsolationLevel isolationLevel);
	}
}
