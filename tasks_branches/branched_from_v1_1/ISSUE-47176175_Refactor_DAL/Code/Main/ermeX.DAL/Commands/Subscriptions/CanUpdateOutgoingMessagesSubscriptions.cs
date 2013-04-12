using System;
using System.Linq.Expressions;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Commands.Observers;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces.Observer;
using ermeX.DAL.Interfaces.Observers;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Subscriptions
{

	
	class CanUpdateOutgoingMessagesSubscriptions : ICanUpdateOutgoingMessagesSubscriptions
	{
		private readonly IPersistRepository<OutgoingMessageSuscription> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;
		private readonly DomainNotifier _domainNotifier; //TODO: REMOVE THIS MECHANISM

		[Inject]
		public CanUpdateOutgoingMessagesSubscriptions(
			IPersistRepository<OutgoingMessageSuscription> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings, IDomainObservable domainNotifier )
		{
			_repository = repository;
			_factory = factory;
			_settings = settings;
			_domainNotifier =(DomainNotifier) domainNotifier;
		}


		public void ImportFromOtherComponent(IncomingMessageSuscription susbcription)
		{
			using (var uow = _factory.Create())
			{
				if (!_repository.Any(uow.Session, x => x.BizMessageFullTypeName == susbcription.BizMessageFullTypeName
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
			bool isNew = false;
			var subscriptionToSave = susbcription;

			using (var uow = _factory.Create())
			{

				Expression<Func<OutgoingMessageSuscription, bool>> expression =
					x => x.Component == susbcription.ComponentOwner && x.BizMessageFullTypeName == susbcription.BizMessageFullTypeName;

				if (!_repository.Any(uow.Session, expression))
				{
					subscriptionToSave.Id = 0;
					isNew = true;
				}
				else
				{
					var existing = _repository.Single(uow.Session, expression);
					subscriptionToSave.Id = existing.Id;
					uow.Session.Evict(existing);
				}
				_repository.Save(uow.Session, subscriptionToSave);
				uow.Commit();

			}

			//TODO: THIS AND ITS MECHANISM MUST BE REMOVED
			_domainNotifier.Notify(isNew ? NotifiableDalAction.Add : NotifiableDalAction.Update, subscriptionToSave);
		}
	}
}