// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ProtoBuf;
using ermeX.Common;
using ermeX.Models.Entities;

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