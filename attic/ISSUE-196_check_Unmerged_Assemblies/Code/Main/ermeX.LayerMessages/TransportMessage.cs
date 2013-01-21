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