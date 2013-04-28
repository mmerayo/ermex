using System;
using System.Collections.Generic;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces.Services
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
		ServiceDetails GetByOperationId(Guid publisher, Guid operationId); //TODO: ISSUE-281: All this Guids to be concrete types

		IEnumerable<ServiceDetails> GetByInterfaceType(Type interfaceType);
		IEnumerable<ServiceDetails> GetByInterfaceType(string interfaceTypeFullName);
		IEnumerable<ServiceDetails> GetByMethodName(string interfaceTypeName, string methodName);
		ServiceDetails GetByMethodName(string interfaceTypeName, string methodName, Guid publisherComponent);
		IEnumerable<ServiceDetails> GetLocalCustomServices();
	}
}
