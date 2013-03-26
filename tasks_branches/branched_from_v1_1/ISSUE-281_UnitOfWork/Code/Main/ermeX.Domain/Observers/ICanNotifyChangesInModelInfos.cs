using ermeX.Entities.Entities;

namespace ermeX.Domain.Observers
{
	

	interface ICanNotifyChangesInModelInfos<TModelInfo>
	{
		void AddObserver(IDomainObserver<TModelInfo> domainObserver);
		void RemoveObserver(IDomainObserver<TModelInfo> messageCollector);
	}

	interface ICanNotifyChangesInOutgoingSubscriptions : ICanNotifyChangesInModelInfos<OutgoingMessageSuscription>
	{
	}
}
