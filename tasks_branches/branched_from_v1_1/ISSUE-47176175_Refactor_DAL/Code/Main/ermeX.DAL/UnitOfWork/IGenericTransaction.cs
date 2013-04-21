using System;

namespace ermeX.DAL.UnitOfWork
{
	public interface IGenericTransaction : IDisposable
	{
		void Commit();
		void Rollback();
	}
}