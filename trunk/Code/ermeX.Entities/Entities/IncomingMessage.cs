// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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