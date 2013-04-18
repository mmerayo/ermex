using System;
using System.Linq.Expressions;
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Commands.Observers;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.Observer;
using ermeX.DAL.Interfaces.Observers;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Subscriptions
{

	
	class CanUpdateOutgoingMessagesSubscriptions : ICanUpdateOutgoingMessagesSubscriptions
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CanUpdateOutgoingMessagesSubscriptions).FullName);
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
			Logger.Debug("cctor");
			_repository = repository;
			_factory = factory;
			_settings = settings;
			_domainNotifier =(DomainNotifier) domainNotifier;
		}


		public void ImportFromOtherComponent(IncomingMessageSuscription susbcription)
		{
			Logger.DebugFormat("ImportFromOtherComponent. susbcription={0}", susbcription);
			using (var uow = _factory.Create())
			{
				if (!_repository.Any(uow, x => x.BizMessageFullTypeName == susbcription.BizMessageFullTypeName
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
			Logger.DebugFormat("ImportFromOtherComponent. susbcription={0}", susbcription);
			bool isNew = false;
			var subscriptionToSave = susbcription;

			using (var uow = _factory.Create())
			{

				Expression<Func<OutgoingMessageSuscription, bool>> expression =
					x => x.Component == susbcription.ComponentOwner && x.BizMessageFullTypeName == susbcription.BizMessageFullTypeName;

				if (!_repository.Any(uow, expression))
				{
					subscriptionToSave.Id = 0;
					isNew = true;
				}
				else
				{
					var existing = _repository.Single(uow, expression);
					subscriptionToSave.Id = existing.Id;
					uow.Session.Evict(existing);
				}
				_repository.Save(uow, subscriptionToSave);
				uow.Commit();

			}

			//TODO: THIS AND ITS MECHANISM MUST BE REMOVED
			_domainNotifier.Notify(isNew ? NotifiableDalAction.Add : NotifiableDalAction.Update, subscriptionToSave);
		}
	}
}