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
using ermeX.LayerMessages;

namespace ermeX.Entities.Entities
{
    [Serializable]
    internal class IncomingMessage : Message, IEquatable<IncomingMessage>
    {
        public const string FinalTableName = "IncomingMessages";

        public IncomingMessage()
        {
        }

//for testing

        public IncomingMessage(BusMessageData message)
            : base(message)
        {
        }

        public virtual DateTime TimeReceivedUtc { get; set; }

        protected override string TableName
        {
            get { return FinalTableName; }
        }

        public virtual Guid SuscriptionHandlerId { get; set; }

        protected internal static string GetDbFieldName(string fieldName)
        {
            return String.Format("{0}_{1}", FinalTableName, fieldName);
        }

        //public static IncomingMessage From(OutgoingMessage source)
        //{
        //    var result = new IncomingMessage(source.BusMessage)
        //                     {
        //                         SerializedFileName = source.SerializedFileName,
        //                         TimePublishedUtc = source.TimePublishedUtc,
        //                         PublishedBy = source.PublishedBy,
        //                         PublishedTo = source.PublishedTo,
        //                     };

        //    return result;
        //}

        public virtual IncomingMessage GetClone()
        {
            var result = new IncomingMessage()
                             {
                                 Version=Version,
                                 ComponentOwner = ComponentOwner,
                                 BusMessageId = BusMessageId,
                                 TimePublishedUtc = TimePublishedUtc,
                                 PublishedBy = PublishedBy,
                                 PublishedTo = PublishedTo,
                                 TimeReceivedUtc = TimeReceivedUtc,
                                 SuscriptionHandlerId = SuscriptionHandlerId
                             };

            return result;
        }

        public static IncomingMessage FromDataRow(DataRow dataRow)
        {
            var result = new IncomingMessage
                             {
                                 Id = Convert.ToInt32( dataRow[GetDbFieldName("Id")]),
                                 BusMessageId = Convert.ToInt32(dataRow[GetDbFieldName("BusMessageId")]),
                                 TimePublishedUtc = new DateTime((long) dataRow[GetDbFieldName("TimePublishedUtc")]),
                                 TimeReceivedUtc = new DateTime((long) dataRow[GetDbFieldName("TimeReceivedUtc")]),
                                 PublishedBy = (Guid) dataRow[GetDbFieldName("PublishedBy")],
                                 PublishedTo = (Guid) dataRow[GetDbFieldName("PublishedTo")],
                                 SuscriptionHandlerId = (Guid) dataRow[GetDbFieldName("SuscriptionHandlerId")],
                                 ComponentOwner = (Guid) dataRow[GetDbFieldName("ComponentOwner")],
                                 Version = (long) dataRow[GetDbFieldName("Version")],
                                 //TODO: TO BASE CLASS
                             };
            return result;
        }

        #region Equatable

        //TODO: refactor to base

        public virtual bool Equals(IncomingMessage other)
        {
            if (other == null)
                return false;

            return BusMessageId == other.BusMessageId
                   && ComponentOwner == other.ComponentOwner && SuscriptionHandlerId == other.SuscriptionHandlerId &&
                   Version == other.Version; //TODO: FINISH
        }

        public static bool operator ==(IncomingMessage a, IncomingMessage b)
        {
            if ((object) a == null || ((object) b) == null)
                return Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(IncomingMessage a, IncomingMessage b)
        {
            if (a == null || b == null)
                return !Equals(a, b);

            return !(a.Equals(b));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (IncomingMessage)) return false;
            return Equals((IncomingMessage) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}