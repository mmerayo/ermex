using System;

namespace ermeX.DAL.DataAccess.UnitOfWork
{
	public interface IGenericTransaction : IDisposable
	{
		void Commit();
		void Rollback();
	}
}