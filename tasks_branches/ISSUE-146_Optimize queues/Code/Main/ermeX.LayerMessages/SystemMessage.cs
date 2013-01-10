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
    internal abstract class SystemMessage : ISystemMessage, IEquatable<SystemMessage>
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

        #region Equatable

        public bool Equals(SystemMessage other)
        {
            if (other == null)
                return false;

            return MessageId==other.MessageId && CreatedTimeUtc==other.CreatedTimeUtc;
        }

        public static bool operator ==(SystemMessage a, SystemMessage b)
        {
            if ((object)a == null || ((object)b) == null)
                return Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(SystemMessage a, SystemMessage b)
        {
            if (a == null || b == null)
                return !Equals(a, b);

            return !(a.Equals(b));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(SystemMessage)) return false;
            return Equals((SystemMessage)obj);
        }

        public override int GetHashCode()
        {
            return MessageId.GetHashCode();
        }

        #endregion
    }
}