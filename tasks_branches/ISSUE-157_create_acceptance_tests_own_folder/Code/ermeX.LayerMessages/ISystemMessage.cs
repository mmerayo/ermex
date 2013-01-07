// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;

namespace ermeX.LayerMessages
{
    internal interface ISystemMessage
    {
        /// <summary>
        /// The message unique identifier
        /// </summary>
        /// <remarks>This must be unique accross the whole system</remarks>
        Guid MessageId { get; }

        DateTime CreatedTimeUtc { get; }
        
    }

    /// <summary>
    /// Represents a system message
    /// </summary>
    internal interface ISystemMessage<TMessage> : ISystemMessage
    {
        /// <summary>
        /// The layer inner data
        /// </summary>
        TMessage Data { get; }
    }
}