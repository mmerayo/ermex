using NHibernate;

namespace ermeX.DAL.Transactions
{
	internal interface ITransactionProvider
	{
		IErmexTransaction BeginTransaction(ITransaction innerTransaction);
	}
}
