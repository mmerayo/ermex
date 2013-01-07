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
    /// Represents a message at the TransportLayer level
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    internal sealed class TransportMessage : SystemMessage, ISystemMessage<BusMessage>
    {
        //just for the serializer, remove in the future
        private TransportMessage()
        {
        }

        public TransportMessage(Guid recipient, BusMessage data)
            : this(data.MessageId, data.CreatedTimeUtc, recipient, data)
        {
        }

        public TransportMessage(Guid messageId, DateTime createdTimeUtc, Guid recipient, BusMessage data)
            : base(messageId, createdTimeUtc)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (recipient.IsEmpty()) throw new ArgumentException("recipient cannot be an empty value");
            Recipient = recipient;
            Data = data;
        }
        
        [ProtoMember(1)]
        public Guid Recipient { get; private set; }

        [ProtoMember(2)]
        public BusMessage Data { get; private set; }
    }
}