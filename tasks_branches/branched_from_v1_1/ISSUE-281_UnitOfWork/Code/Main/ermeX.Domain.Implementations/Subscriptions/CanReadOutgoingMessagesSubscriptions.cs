using System.Collections.Generic;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Subscriptions;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Subscriptions
{
	class CanReadOutgoingMessagesSubscriptions : ICanReadOutgoingMessagesSubscriptions
	{
		private readonly IOutgoingMessageSuscriptionsDataSource _repository;

		[Inject]
		public CanReadOutgoingMessagesSubscriptions(IOutgoingMessageSuscriptionsDataSource repository)
		{
			_repository = repository;
		}

		public IList<OutgoingMessageSuscription> GetByMessageType(string bizMessageType)
		{
			return _repository.GetByMessageType(bizMessageType);//TODO: move logic here
		}
	}
}