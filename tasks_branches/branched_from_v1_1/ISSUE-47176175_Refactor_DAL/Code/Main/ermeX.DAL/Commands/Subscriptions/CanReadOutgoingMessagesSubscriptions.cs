using System.Collections.Generic;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Subscriptions
{
	class CanReadOutgoingMessagesSubscriptions : ICanReadOutgoingMessagesSubscriptions
	{
		private readonly IReadOnlyRepository<OutgoingMessageSuscription> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;

		[Inject]
		public CanReadOutgoingMessagesSubscriptions(IReadOnlyRepository<OutgoingMessageSuscription> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
		{
			_repository = repository;
			_factory = factory;
			_settings = settings;
		}

		public IEnumerable<OutgoingMessageSuscription> GetByMessageType(string bizMessageType)
		{
			IEnumerable<OutgoingMessageSuscription> result;
			using (var ouw = _factory.Create())
			{
				result=_repository.Where(x => x.BizMessageFullTypeName == bizMessageType);
				ouw.Commit();
			}
			return result;

		}

		public IEnumerable<OutgoingMessageSuscription> FetchAll()
		{
			IEnumerable<OutgoingMessageSuscription> result;
			using (var ouw = _factory.Create())
			{
				result = _repository.FetchAll();
				ouw.Commit();
			}
			return result;
		}
	}
}