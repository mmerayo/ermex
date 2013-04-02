using System;

namespace ermeX.DAL.DataAccess.UoW
{
	public interface IGenericTransaction : IDisposable
	{
		void Commit();
		void Rollback();
	}
}