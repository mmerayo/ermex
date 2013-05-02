namespace ermeX.DAL.Interfaces.Observer
{

	//TODO: REMOVE THIS type and its everything
    internal interface IDalObserver<TEntity>
    {
        void Notify(NotifiableDalAction action, TEntity entity);
    }
}
