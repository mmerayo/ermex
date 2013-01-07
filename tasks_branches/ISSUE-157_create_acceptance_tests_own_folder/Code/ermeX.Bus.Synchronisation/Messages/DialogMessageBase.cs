// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Common;

namespace ermeX.Bus.Synchronisation.Messages
{
    internal abstract class DialogMessageBase
    {
        protected DialogMessageBase(){} //TODO: resolve restriction on deserializer when there is not an empty constructor
        protected DialogMessageBase(Guid sourceComponentId) : this(sourceComponentId, Guid.NewGuid())
        {
        }

        protected DialogMessageBase(Guid sourceComponentId, Guid correlationId)
        {
            if (sourceComponentId.IsEmpty() || correlationId.IsEmpty())
                throw new ArgumentException("parameter cannot be empty");
            SourceComponentId = sourceComponentId;
            DateCreated = DateTime.UtcNow;
            CorrelationId = correlationId;
        }

        public Guid SourceComponentId { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid CorrelationId { get; set; }
    }
}