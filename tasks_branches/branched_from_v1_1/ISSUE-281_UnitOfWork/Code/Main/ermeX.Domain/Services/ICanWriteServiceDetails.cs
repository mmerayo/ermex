using ermeX.Entities.Entities;

namespace ermeX.Domain.Services
{
	interface ICanWriteServiceDetails
	{
		//TODO: ISSUE-281: RENAME WRITE TO STORE
		void ImportFromOtherComponent(ServiceDetails svc);
	}
}