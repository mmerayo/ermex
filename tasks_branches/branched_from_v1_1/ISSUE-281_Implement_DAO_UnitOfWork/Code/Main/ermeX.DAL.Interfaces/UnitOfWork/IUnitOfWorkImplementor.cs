namespace ermeX.DAL.Interfaces.UnitOfWork
{
    internal interface IUnitOfWorkImplementor : IUnitOfWork
    {
        void IncrementUsages();
    }
}