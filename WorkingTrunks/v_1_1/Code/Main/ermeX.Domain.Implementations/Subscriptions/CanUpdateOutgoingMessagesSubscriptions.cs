using System;
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


		public void ImportFromOtherComponent(IncomingMessageSuscription susbcription)
		{
			_repository.SaveFromOtherComponent(susbcription);//TODO: move logic here
		}

		public void ImportFromOtherComponent(OutgoingMessageSuscription susbcription)
		{
			//ISSUE-281: cleanup this code
			var deterministicFilter = new[]
                        {
                            new Tuple<string, object>("BizMessageFullTypeName",
                                                      susbcription.BizMessageFullTypeName),
                            new Tuple<string, object>("Component", susbcription.Component)
                        };

			_repository.SaveFromOtherComponent(susbcription, deterministicFilter);
		}
	}
}