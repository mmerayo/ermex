namespace ermeX.DAL.Interfaces.Observers
{
	interface IDomainObservable
	{
		void AddObserver<TModelInfo>(IDomainObserver<TModelInfo> observer);
		void RemoveObserver<TModelInfo>(IDomainObserver<TModelInfo> observer);
	}
}
