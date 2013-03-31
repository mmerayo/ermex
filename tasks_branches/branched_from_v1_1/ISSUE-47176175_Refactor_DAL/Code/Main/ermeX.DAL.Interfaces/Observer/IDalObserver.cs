namespace ermeX.DAL.Interfaces.Observer
{
    internal interface IDalObserver<TEntity>
    {
        void Notify(NotifiableDalAction action, TEntity entity);
    }
}
