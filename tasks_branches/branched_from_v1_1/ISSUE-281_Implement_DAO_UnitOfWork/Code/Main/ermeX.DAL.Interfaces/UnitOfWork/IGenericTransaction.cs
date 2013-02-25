using System;

namespace ermeX.DAL.Interfaces.UnitOfWork
{
    internal interface IGenericTransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}