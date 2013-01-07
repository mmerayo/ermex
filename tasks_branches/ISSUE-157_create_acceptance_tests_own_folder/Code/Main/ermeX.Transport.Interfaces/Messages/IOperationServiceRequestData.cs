// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;

namespace ermeX.Transport.Interfaces.Messages
{
    internal interface IOperationServiceRequestData : IServiceRequestMessage
    {
        Guid ServerId { get; }

        /// <summary>
        ///   The calling context for the operation. allows to trace responses.
        /// </summary>
        /// <remarks>
        ///   Guid.Empty when no reponse is expected
        /// </remarks>
        Guid CallingContextId { get; }

        IDictionary<string, ServiceRequestMessage.RequestParameter> Parameters { get; }
        string ResultType { get; }
    }
}