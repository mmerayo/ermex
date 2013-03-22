using System;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Services
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
	}
}