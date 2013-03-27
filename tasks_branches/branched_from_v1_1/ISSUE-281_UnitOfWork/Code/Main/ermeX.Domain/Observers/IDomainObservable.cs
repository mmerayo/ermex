using ermeX.Entities.Entities;

namespace ermeX.Domain.Observers
{
	interface IDomainObservable
	{
		void AddObserver<TModelInfo>(IDomainObserver<TModelInfo> observer);
		void RemoveObserver<TModelInfo>(IDomainObserver<TModelInfo> observer);
	}
}
