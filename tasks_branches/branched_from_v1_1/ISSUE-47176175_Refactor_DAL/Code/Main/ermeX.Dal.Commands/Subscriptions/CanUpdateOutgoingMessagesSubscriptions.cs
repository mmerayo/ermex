using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Commands.Observers;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
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
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_repository = repository;
			_factory = factory;
			_settings = settings;
			_domainNotifier =(DomainNotifier) domainNotifier;
		}

		//TODO: THIS MUST BE REMOVED AND MADE BY THE CLIENT CODE
		public void ImportFromOtherComponent(IncomingMessageSuscription susbcription)
		{
			Logger.TraceFormat("ImportFromOtherComponent. susbcription={0} AppDomain={1} - Thread={2}", susbcription, AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);

			_factory.ExecuteInUnitOfWork(false, uow => ImportForeignIncomming(uow, susbcription));
		}

		private void ImportForeignIncomming( IUnitOfWork uow,IncomingMessageSuscription susbcription)
		{
			if (!_repository.Any(uow, x => x.BizMessageFullTypeName == susbcription.BizMessageFullTypeName
			                               && x.Component == susbcription.ComponentOwner
			                               && x.ComponentOwner == _settings.ComponentId))
			{
				var subscriptionToSave = new OutgoingMessageSuscription(susbcription, susbcription.ComponentOwner,
				                                                        _settings.ComponentId);

				_repository.Save(uow, subscriptionToSave);
			}
		}


		public void ImportFromOtherComponent(IEnumerable<IncomingMessageSuscription> incomingSuscriptions)
		{
			Logger.TraceFormat("ImportFromOtherComponent. enumeration AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);

			_factory.ExecuteInUnitOfWork(false, uow =>
				{
					foreach (var s in incomingSuscriptions)
						ImportForeignIncomming(uow, s);
				});
		}

		//TODO: THIS MUST BE REMOVED AND MADE BY THE CLIENT CODE
		public void ImportFromOtherComponent(OutgoingMessageSuscription susbcription)
		{
			Logger.TraceFormat("ImportFromOtherComponent. susbcription={0} AppDomain={1} - Thread={2}", susbcription, AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId;
			_factory.ExecuteInUnitOfWork(false, uow => ImportFromOtherComponent(uow, susbcription));
		}

		private void ImportFromOtherComponent( IUnitOfWork uow, OutgoingMessageSuscription susbcription)
		{
			susbcription.Id = 0;
			susbcription.ComponentOwner = _settings.ComponentId;
			_repository.Save(uow, susbcription);
			//TODO: THIS AND ITS MECHANISM MUST BE REMOVED when state machine is in place

			//_domainNotifier.Notify(isNew ? NotifiableDalAction.Add : NotifiableDalAction.Update,susbcription);
		}


		public void ImportFromOtherComponent(IEnumerable<OutgoingMessageSuscription> remoteSuscriptions)
		{
			Logger.TraceFormat("ImportFromOtherComponent. several AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
			if (remoteSuscriptions == null) throw new ArgumentNullException("remoteSuscriptions");
			
			//discard the local suscriptions
			var outgoingMessageSuscriptions = remoteSuscriptions.Where(x => x.Component != _settings.ComponentId);
			
			_factory.ExecuteInUnitOfWork(false,uow =>
				{
					foreach (var outgoingMessageSuscription in outgoingMessageSuscriptions)
						ImportFromOtherComponent(uow,outgoingMessageSuscription);
				});

		}
	}
}