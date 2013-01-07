// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces
{
    internal interface IServiceDetailsDataSource : IDataSource<ServiceDetails>
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
        ServiceDetails GetByMethodName(string interfaceTypeName, string methodName);
        ServiceDetails GetByMethodName(string interfaceTypeName, string methodName, Guid publisherComponent);
        IList<ServiceDetails> GetLocalCustomServices();
    }
}