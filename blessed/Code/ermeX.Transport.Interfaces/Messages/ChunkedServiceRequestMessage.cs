// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ProtoBuf;
using ermeX.Common;
using ermeX.Entities.Entities;

namespace ermeX.Transport.Interfaces.Messages
{
    [ProtoContract(SkipConstructor = true)]
    internal class ChunkedServiceRequestMessage : ChunkedServiceRequestMessageData
    {
        private ChunkedServiceRequestMessage()
        {
        }

        public ChunkedServiceRequestMessage(Guid correlationId, int order, bool eof, Guid operationIdentifier,
                                            byte[] message):this()
        {
            if (operationIdentifier == null || operationIdentifier == Guid.Empty)
                throw new ArgumentNullException("operationIdentifier");
            if (message == null) throw new ArgumentNullException("message");
            Order = order;
            Eof = eof;
            CorrelationId = correlationId;

            Operation = operationIdentifier;
            Data = new List<byte>(message);
        }
        [ProtoMember(1)]
        public override Guid Operation { get; set; }
        
        [ProtoMember(2)]
        public override List<byte> Data { get; set; }//TODO: DUE TO PROBLEMS SERIALIZING REMOVE(fROM BASE TOO) AND RETRY

        [ProtoMember(3)]
        public override Guid CorrelationId { get; set; }

        [ProtoMember(4)]
        public override int Order { get; set; }

        [ProtoMember(5)]
        public override bool Eof { get; set; }

       

        public static ChunkedServiceRequestMessage FromData(ChunkedServiceRequestMessageData source )
        {
            return new ChunkedServiceRequestMessage(source.CorrelationId,source.Order,source.Eof,source.Operation,source.Data.ToArray());
        }
    }
}