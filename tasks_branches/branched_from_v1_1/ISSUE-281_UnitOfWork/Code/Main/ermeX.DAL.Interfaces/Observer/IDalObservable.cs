namespace ermeX.DAL.Interfaces.Observer
{
    internal interface IDalObservable<TEntity>
    {
        void AddObserver(IDalObserver<TEntity> observer);
        void RemoveObserver(IDalObserver<TEntity> observer);
        void Notify(NotifiableDalAction action, TEntity entity);
    }
}