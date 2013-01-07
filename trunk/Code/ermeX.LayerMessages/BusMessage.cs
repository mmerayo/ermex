// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ProtoBuf;
using ermeX.Common;

namespace ermeX.LayerMessages
{
    /// <summary>
    /// Represents a message at the BusLayer level
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    internal sealed class BusMessage: SystemMessage, ISystemMessage<BizMessage>
    {
        private BusMessage()
        {
            
        }
        public BusMessage(Guid publisher,BizMessage data)
            : this(data.MessageId,data.CreatedTimeUtc,publisher, data)
        {
            

        }

        public BusMessage(Guid messageId, DateTime createdTimeUtc, Guid publisher, BizMessage data) : base(messageId,createdTimeUtc)
        {
            if (data == null) throw new ArgumentNullException("data");
            Publisher = publisher;
            Data = data;
        }
        [ProtoMember(1)]
        public Guid Publisher { get; protected set; }

        [ProtoMember(2)]
        public BizMessage Data { get; protected set; }


    }
}