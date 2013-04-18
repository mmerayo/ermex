using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Subscriptions
{
	class CanReadOutgoingMessagesSubscriptions : ICanReadOutgoingMessagesSubscriptions
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CanReadOutgoingMessagesSubscriptions).FullName);

		private readonly IReadOnlyRepository<OutgoingMessageSuscription> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;

		[Inject]
		public CanReadOutgoingMessagesSubscriptions(IReadOnlyRepository<OutgoingMessageSuscription> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
		{
			Logger.Debug("cctor");
			_repository = repository;
			_factory = factory;
			_settings = settings;
		}

		public IEnumerable<OutgoingMessageSuscription> GetByMessageType(string bizMessageType)
		{
			Logger.DebugFormat("GetByMessageType. bizMessageType={0}", bizMessageType);
			IEnumerable<OutgoingMessageSuscription> result;
			using (var uow = _factory.Create())
			{
				result = _repository.Where(uow, x => x.BizMessageFullTypeName == bizMessageType).ToList();
				uow.Commit();
			}
			return result;

		}

		public IEnumerable<OutgoingMessageSuscription> FetchAll()
		{
			Logger.Debug("FetchAll");
			IEnumerable<OutgoingMessageSuscription> result;
			using (var uow = _factory.Create())
			{
				result = _repository.FetchAll(uow).ToList();
				uow.Commit();
			}
			return result;
		}
	}
}