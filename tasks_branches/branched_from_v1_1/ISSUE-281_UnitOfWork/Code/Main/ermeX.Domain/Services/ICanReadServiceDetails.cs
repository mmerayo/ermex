using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Services
{
	interface ICanReadServiceDetails
	{
		/// <summary>
		///   Gets the local server service details
		/// </summary>
		/// <param name="operationId"> </param>
		/// <returns> </returns>
		ServiceDetails GetByOperationId(Guid operationId);

		/// <summary>
		///   Gets any server service details
		/// </summary>
		/// <param name="operationId"> </param>
		/// <returns> </returns>
		ServiceDetails GetByOperationId(Guid publisher, Guid operationId);

		IList<ServiceDetails> GetByInterfaceType(Type interfaceType);
		IList<ServiceDetails> GetByInterfaceType(string interfaceTypeFullName);
		IList<ServiceDetails> GetByMethodName(string interfaceTypeName, string methodName);
		ServiceDetails GetByMethodName(string interfaceTypeName, string methodName, Guid publisherComponent);
		IList<ServiceDetails> GetLocalCustomServices();
	}
}
