using System;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Services;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Services
{
	class ServiceDetailsWriter : ICanWriteServiceDetails
	{
		private readonly IServiceDetailsDataSource _repository;
		
		[Inject]
		public ServiceDetailsWriter(IServiceDetailsDataSource repository)
		{
			_repository = repository;
		}
		public void ImportFromOtherComponent(ServiceDetails svc)
		{
			//TODO: ISSUE-281: IMPROVE THIS KIND OF CALLS
			var deterministicFilter = new[]
                        {
                            new Tuple<string, object>("OperationIdentifier", svc.OperationIdentifier),
                            new Tuple<string, object>("Publisher", svc.Publisher)
                        };
			_repository.SaveFromOtherComponent(svc, deterministicFilter);
		}

		public void Save(ServiceDetails serviceDetails)
		{
			_repository.Save(serviceDetails);
		}
	}
}