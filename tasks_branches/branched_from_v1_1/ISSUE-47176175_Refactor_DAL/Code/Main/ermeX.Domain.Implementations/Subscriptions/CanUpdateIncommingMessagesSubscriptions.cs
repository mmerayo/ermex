using System;
using System.Collections.Generic;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Subscriptions;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Subscriptions
{
	internal class CanUpdateIncommingMessagesSubscriptions : ICanUpdateIncommingMessagesSubscriptions
	{
		private readonly IIncomingMessageSuscriptionsDataSource _repository;

		[Inject]
		public CanUpdateIncommingMessagesSubscriptions(IIncomingMessageSuscriptionsDataSource repository)
		{
			_repository = repository;
		}

		public void RemoveByHandlerId(Guid suscriptionId)
		{
			_repository.RemoveByHandlerId(suscriptionId);//TODO: move logic here
		}

		public void SaveIncommingSubscription(Guid suscriptionHandlerId, Type handlerType, Type messageType)
		{
			_repository.SaveIncommingSubscription(suscriptionHandlerId,handlerType,messageType);//TODO: move logic here
		}

		public void Remove(List<IncomingMessageSuscription> toRemove)
		{
			_repository.Remove(toRemove);//TODO: move logic here
		}
	}
}