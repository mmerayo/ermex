// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Linq;
using ProtoBuf;
using ProtoBuf.Meta;
using ermeX.Common;

namespace ermeX.LayerMessages
{
    [ProtoContract(SkipConstructor = true)]
    [ProtoInclude(100, typeof(BusMessage))]
    [ProtoInclude(200, typeof(TransportMessage))]
    [ProtoInclude(300, typeof(BizMessage))]
    internal abstract class SystemMessage:ISystemMessage
    {
        protected SystemMessage():this(Guid.NewGuid(),DateTime.UtcNow)
        {
        }
        protected SystemMessage(Guid messageId,DateTime createdTimeUtc)
        {
            MessageId = messageId;
            CreatedTimeUtc = new DateTime( createdTimeUtc.Ticks);//TODO: UNTIL PROTOBUF-NET FIXES ISSUE 335 RAISED BY ME
        }

        [ProtoMember(1)]
        public  Guid MessageId{get;private set;}

        [ProtoMember(2)]
        public DateTime CreatedTimeUtc { get; private set; }
    }
}