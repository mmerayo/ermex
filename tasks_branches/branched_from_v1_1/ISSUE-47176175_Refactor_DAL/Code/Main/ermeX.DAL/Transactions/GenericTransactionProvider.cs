using System;
using NHibernate;

namespace ermeX.DAL.Transactions
{
	internal sealed class GenericTransactionProvider : ITransactionProvider
	{
		public IErmexTransaction BeginTransaction(ITransaction innerTransaction)
		{
			if (innerTransaction == null) throw new ArgumentNullException("innerTransaction");
			return new ErmexTransaction(innerTransaction);
		}
	}
}