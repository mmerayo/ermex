using System;
using System.Collections.Generic;
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Subscriptions
{
	internal class CanUpdateIncommingMessagesSubscriptions : ICanUpdateIncommingMessagesSubscriptions
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CanUpdateIncommingMessagesSubscriptions).FullName);

		private readonly IPersistRepository<IncomingMessageSuscription> _incomingRepository;
		private readonly IPersistRepository<OutgoingMessageSuscription> _outgoingRepository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;

		[Inject]
		public CanUpdateIncommingMessagesSubscriptions(IPersistRepository<IncomingMessageSuscription> incommingRepository,
			IPersistRepository<OutgoingMessageSuscription> outgoingRepository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
		{
			Logger.Debug("cctor");
			_incomingRepository = incommingRepository;
			_outgoingRepository = outgoingRepository;
			_factory = factory;
			_settings = settings;
		}

		public void RemoveByHandlerId(Guid suscriptionId)
		{
			Logger.DebugFormat("RemoveByHandlerId. suscriptionId={0}", suscriptionId);
			using (var uow = _factory.Create())
			{
				_incomingRepository.Remove(uow, x => x.SuscriptionHandlerId == suscriptionId);
				uow.Commit();
			}
		}

		public void SaveIncommingSubscription(Guid suscriptionHandlerId, Type handlerType, Type messageType)
		{
			Logger.DebugFormat("SaveIncommingSubscription. suscriptionHandlerId={0} handlerType={1} messageType={2}", suscriptionHandlerId, handlerType, messageType);
			using (var uow = _factory.Create())
			{
				var incomingMessageSuscription = new IncomingMessageSuscription
				{
					ComponentOwner = _settings.ComponentId,
					BizMessageFullTypeName = messageType.FullName,
					DateLastUpdateUtc = DateTime.UtcNow,
					SuscriptionHandlerId = suscriptionHandlerId,
					HandlerType = handlerType.FullName
				};
				_incomingRepository.Save(uow, incomingMessageSuscription);
				//TODO: TO BE MOVED TO THE OUTGOING SUSCRIPTIONS UPDATER

				if (!_outgoingRepository.Any(uow, x => x.BizMessageFullTypeName == messageType.FullName && x.Component == _settings.ComponentId))
				{
					var outgoingMessageSuscription = new OutgoingMessageSuscription(incomingMessageSuscription, _settings.ComponentId, _settings.ComponentId);
					_outgoingRepository.Save(uow, outgoingMessageSuscription);
				}
				uow.Commit();
			}
		}

		public void Remove(IEnumerable<IncomingMessageSuscription> toRemove)
		{
			using (var uow = _factory.Create())
			{
				_incomingRepository.Remove(uow, toRemove);
				uow.Commit();
			}
		}
	}
}