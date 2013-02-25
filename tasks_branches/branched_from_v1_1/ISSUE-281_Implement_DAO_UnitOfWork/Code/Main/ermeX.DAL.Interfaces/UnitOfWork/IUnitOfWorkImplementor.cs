namespace ermeX.DAL.Interfaces.UnitOfWork
{
    public interface IUnitOfWorkImplementor : IUnitOfWork
    {
        void IncrementUsages();
    }
}