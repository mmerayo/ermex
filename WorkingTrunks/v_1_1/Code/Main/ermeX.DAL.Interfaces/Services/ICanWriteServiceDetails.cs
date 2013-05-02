using ermeX.Models.Entities;

namespace ermeX.DAL.Interfaces.Services
{
	interface ICanWriteServiceDetails
	{
		//TODO: ISSUE-281: RENAME WRITE TO STORE
		void ImportFromOtherComponent(ServiceDetails svc);
		void Save(ServiceDetails serviceDetails);
	}
}