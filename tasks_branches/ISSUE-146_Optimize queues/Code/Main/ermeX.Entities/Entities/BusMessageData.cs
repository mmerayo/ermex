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
using System.Data;
using ermeX.Entities.Base;
using ermeX.LayerMessages;
using BaseBusMessage= ermeX.LayerMessages.BusMessage;
namespace ermeX.Entities.Entities
{
    internal class  BusMessageData:ModelBase,IEquatable<BusMessageData>
    {
        public enum BusMessageStatus:int
        {
            /// <summary>
            /// This is an special stutus to save the first stage, no copies created per subscriber yet
            /// </summary>
            SenderCollected=1,
            /// <summary>
            /// Is ready to deliver to subscriber. Its refered by outpoing message
            /// </summary>
            SenderDispatchPending=2,
            /// <summary>
            /// Marks the message as sent
            /// </summary>
            SenderSent,

            /// <summary>
            /// Received but not created copy per local subscription
            /// </summary>
            ReceiverReceived,
            /// <summary>
            /// Ready to be delivered to the handler
            /// </summary>
            ReceiverDispatchable,

            /// <summary>
            /// its being dispatched now
            /// </summary>
            ReceiverDispatching


        }

        public BusMessageData()
        {
        }

        internal BusMessageData(Guid messageId, DateTime createdTimeUtc, Guid publisher, string jsonMessage, BusMessageStatus status)
        {
            MessageId = messageId;
            CreatedTimeUtc = createdTimeUtc;
            Publisher = publisher;
            JsonMessage = jsonMessage;
            Status = status;
        }

        public virtual BusMessageStatus Status { get; set; }

        public virtual Guid Publisher { get; set; }

        public virtual string JsonMessage { get; set; }

        public virtual Guid MessageId { get; set; }

        public virtual DateTime CreatedTimeUtc { get; set; }

        public static BusMessageData FromBusLayerMessage(Guid componentId, BusMessage source, BusMessageStatus status)
        {
            var busMessage = new BusMessageData(source.MessageId, source.CreatedTimeUtc, source.Publisher, source.Data.JsonMessage,status)
            {ComponentOwner=componentId};
            return busMessage;
        }

        public static implicit operator BaseBusMessage(BusMessageData source)
        {
            BizMessage bizMessage = BizMessage.FromJson(source.JsonMessage);
            return new BaseBusMessage(source.MessageId, source.CreatedTimeUtc, source.Publisher, bizMessage);
        }

        public static string TableName
        {
            get { return "BusMessages"; }
        }

        #region Equatable

        public virtual bool Equals(BusMessageData other)
        {
            if (other == null)
                return false;

            return Publisher == other.Publisher
                && JsonMessage == other.JsonMessage
                && MessageId == other.MessageId
                && Version == other.Version
                && CreatedTimeUtc == other.CreatedTimeUtc;
        }

        public static bool operator ==(BusMessageData a, BusMessageData b)
        {
            if ((object)a == null || ((object)b) == null)
                return Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(BusMessageData a, BusMessageData b)
        {
            if (a == null || b == null)
                return !Equals(a, b);

            return !(a.Equals(b));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(BusMessageData)) return false;
            return Equals((BusMessageData)obj);
        }

        public override int GetHashCode()
        {
            return MessageId.GetHashCode();
        }

        #endregion

        public static BusMessageData FromDataRow(DataRow dataRow)
        {
            var result = new BusMessageData
            {
                Id = Convert.ToInt32(dataRow[GetDbFieldName("Id")]),
                ComponentOwner = (Guid)dataRow[GetDbFieldName("ComponentOwner")],
                Version = (long)dataRow[GetDbFieldName("Version")],
                Status = (BusMessageStatus)Convert.ToInt32(dataRow[GetDbFieldName("Status")]),
                Publisher = (Guid)dataRow[GetDbFieldName("Publisher")],
                JsonMessage = dataRow[GetDbFieldName("JsonMessage")].ToString(),
                MessageId = (Guid)dataRow[GetDbFieldName("MessageId")],
                CreatedTimeUtc = new DateTime((long)dataRow[GetDbFieldName("CreatedTimeUtc")])
            };
            return result;
        }

        protected internal static string GetDbFieldName(string fieldName)
        {
            return String.Format("{0}_{1}", TableName, fieldName);
        }


        public static BusMessageData NewFromExisting(BusMessageData source)
        {
            var result = new BusMessageData(source.MessageId,source.CreatedTimeUtc,source.Publisher,source.JsonMessage,source.Status)
            {
                ComponentOwner = source.ComponentOwner,
                Version = source.Version
            };
            return result;
        }
    }
}
