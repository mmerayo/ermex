using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Subscriptions;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Subscriptions
{
	class CanUpdateOutgoingMessagesSubscriptions : ICanUpdateOutgoingMessagesSubscriptions
	{
		private readonly IOutgoingMessageSuscriptionsDataSource _repository;

		[Inject]
		public CanUpdateOutgoingMessagesSubscriptions(IOutgoingMessageSuscriptionsDataSource repository)
		{
			_repository = repository;
		}


		public void SaveFromOtherComponent(IncomingMessageSuscription request)
		{
			_repository.SaveFromOtherComponent(request);//TODO: move logic here
		}
	}
}