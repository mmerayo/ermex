namespace ermeX.Common.Observer
{
    internal interface IObservable<TEntity>
    {
        void AddObserver(IObserver<TEntity> observer);
        void RemoveObserver(IObserver<TEntity> observer);
        void Notify(NotifiableDalAction action, TEntity entity);
    }
}