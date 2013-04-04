using System;
using System.Linq.Expressions;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Subscriptions;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Subscriptions
{
	class CanUpdateOutgoingMessagesSubscriptions : ICanUpdateOutgoingMessagesSubscriptions
	{
		private readonly IPersistRepository<OutgoingMessageSuscription> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;

		[Inject]
		public CanUpdateOutgoingMessagesSubscriptions(
			IPersistRepository<OutgoingMessageSuscription> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
		{
			_repository = repository;
			_factory = factory;
			_settings = settings;
		}


		public void ImportFromOtherComponent(IncomingMessageSuscription susbcription)
		{
			using (var uow = _factory.Create())
			{
				if (!_repository.Any(x => x.BizMessageFullTypeName == susbcription.BizMessageFullTypeName
				                          && x.Component == susbcription.ComponentOwner
				                          && x.ComponentOwner == _settings.ComponentId))
				{
					var subscriptionToSave = new OutgoingMessageSuscription(susbcription, susbcription.ComponentOwner,
					                                                        _settings.ComponentId);

					ImportFromOtherComponent(subscriptionToSave);
					uow.Commit();
				}
			}
		}

		public void ImportFromOtherComponent(OutgoingMessageSuscription susbcription)
		{
			using (var uow = _factory.Create())
			{
				var subscriptionToSave = susbcription;

				Expression<Func<OutgoingMessageSuscription, bool>> expression =
					x => x.Component == susbcription.ComponentOwner && x.BizMessageFullTypeName == susbcription.BizMessageFullTypeName;
				if (!_repository.Any(expression))
				{
					subscriptionToSave.Id = 0;
				}
				else
				{
					var existing = _repository.Single(expression);
					subscriptionToSave.Id = existing.Id;
					uow.Session.Evict(existing);
				}
				_repository.Save(subscriptionToSave);
				uow.Commit();
			}
		}
	}
}