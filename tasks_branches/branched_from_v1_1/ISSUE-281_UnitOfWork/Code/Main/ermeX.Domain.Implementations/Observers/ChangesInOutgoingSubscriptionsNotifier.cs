using System;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Observer;
using ermeX.Domain.Observers;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Observers
{
	
	internal sealed class ChangesInOutgoingSubscriptionsNotifier : ICanNotifyChangesInOutgoingSubscriptions, IDalObserver<OutgoingMessageSuscription>
	{
		


		public void AddObserver(IDomainObserver<OutgoingMessageSuscription> domainObserver)
		{
			throw new NotImplementedException();
		}

		public void RemoveObserver(IDomainObserver<OutgoingMessageSuscription> messageCollector)
		{
			throw new NotImplementedException();
		}

		public void Notify(NotifiableDalAction action, OutgoingMessageSuscription entity)
		{
			throw new NotImplementedException();
			switch (action)
			{
				case NotifiableDalAction.Add:
					break;
				case NotifiableDalAction.Update:
					break;
				case NotifiableDalAction.Remove:
					break;
				default:
					throw new ArgumentOutOfRangeException("action");
			}
		}
	}
}
