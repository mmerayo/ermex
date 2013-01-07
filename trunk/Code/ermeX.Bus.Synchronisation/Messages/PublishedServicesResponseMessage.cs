// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ermeX.Entities.Entities;

namespace ermeX.Bus.Synchronisation.Messages
{
    internal sealed class PublishedServicesResponseMessage : DialogMessageBase
    {
        public PublishedServicesResponseMessage(Guid sourceComponentId, Guid correlationId)
            : base(sourceComponentId, correlationId)
        {
        }


        public IEnumerable<ServiceDetails> LocalServiceDefinitions { get; set; }
    }
}