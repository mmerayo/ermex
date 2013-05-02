using System;

namespace ermeX.DAL.Transactions
{
	public interface IErmexTransaction : IDisposable
	{
		void Commit();
		void Rollback();
	}
}