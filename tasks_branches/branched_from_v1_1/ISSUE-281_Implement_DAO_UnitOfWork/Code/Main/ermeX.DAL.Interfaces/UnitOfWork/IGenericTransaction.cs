using System;

namespace ermeX.DAL.Interfaces.UnitOfWork
{
    public interface IGenericTransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}