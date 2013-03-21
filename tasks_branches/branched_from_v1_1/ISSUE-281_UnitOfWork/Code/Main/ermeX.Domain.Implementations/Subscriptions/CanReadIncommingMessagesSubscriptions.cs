using System;
using System.Collections.Generic;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Subscriptions;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Subscriptions
{
	class CanReadIncommingMessagesSubscriptions : ICanReadIncommingMessagesSubscriptions
	{
		private readonly IIncomingMessageSuscriptionsDataSource _repository;

		[Inject]
		public CanReadIncommingMessagesSubscriptions(IIncomingMessageSuscriptionsDataSource repository)
		{
			_repository = repository;
		}

		public IList<IncomingMessageSuscription> GetByMessageType(string bizMessageType)
		{
			return _repository.GetByMessageType(bizMessageType);//TODO: move logic here
		}

		public IncomingMessageSuscription GetByHandlerId(Guid suscriptionHandlerId)
		{
			return _repository.GetByHandlerId(suscriptionHandlerId);//TODO: move logic here
		}

		public IncomingMessageSuscription GetByHandlerAndMessageType(Type handlerType, Type messageType)
		{
			return _repository.GetByHandlerAndMessageType(handlerType,messageType);//TODO: move logic here
		}
	}
}