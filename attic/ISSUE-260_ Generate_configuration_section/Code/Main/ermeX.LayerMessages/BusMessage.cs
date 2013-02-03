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
    /// Represents a message at the BusLayer level
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    internal sealed class BusMessage: SystemMessage, ISystemMessage<BizMessage>,IEquatable<BusMessage>
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


        public static BusMessage Clone(BusMessage busMessage)
        {
            return new BusMessage(busMessage.MessageId,busMessage.CreatedTimeUtc,busMessage.Publisher,busMessage.Data);
        }

        #region Equatable

        public bool Equals(BusMessage other)
        {
            if (other == null)
                return false;

            return base.Equals(other) && Publisher == other.Publisher && Publisher == other.Publisher;
        }

        public static bool operator ==(BusMessage a, BusMessage b)
        {
            if ((object)a == null || ((object)b) == null)
                return Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(BusMessage a, BusMessage b)
        {
            if (a == null || b == null)
                return !Equals(a, b);

            return !(a.Equals(b));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(BusMessage)) return false;
            return Equals((BusMessage)obj);
        }

        public override int GetHashCode()
        {
            return MessageId.GetHashCode();
        }

        #endregion
    }
}