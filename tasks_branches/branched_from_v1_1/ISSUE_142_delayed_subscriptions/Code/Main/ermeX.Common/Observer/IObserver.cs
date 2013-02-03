namespace ermeX.Common.Observer
{
    internal interface IObserver<TEntity>
    {
        void Notify(NotifiableDalAction action, TEntity entity);
    }
}
