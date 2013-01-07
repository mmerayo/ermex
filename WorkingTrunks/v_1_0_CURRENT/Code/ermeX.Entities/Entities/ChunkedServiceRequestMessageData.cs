// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ermeX.Entities.Base;

namespace ermeX.Entities.Entities
{
    internal class ChunkedServiceRequestMessageData:ModelBase,IEquatable<ChunkedServiceRequestMessageData>
    {

        public ChunkedServiceRequestMessageData()
        {
        }
       
        public virtual Guid Operation { get; set; }

        #region Crap due to the serializer was not supporting it, fix when supported

        public virtual List<byte> Data
        {
            get { return new List<byte>(DataBytes); }
            set { DataBytes = value.ToArray(); }
        }

        public virtual byte[] DataBytes { get; set; } 
        #endregion

        public virtual Guid CorrelationId { get; set; }

        public virtual int Order { get; set; }

        public virtual bool Eof { get; set; }

        public static string TableName
        {
            get { return "ChunkedServiceRequestMessages"; }
        }


        #region Equatable

        public virtual bool Equals(ChunkedServiceRequestMessageData other)
        {
            if (other == null)
                return false;

            return Operation == other.Operation
                && CorrelationId == other.CorrelationId
                && Order == other.Order
                && Version == other.Version
                && Eof == other.Eof;
        }

        public static bool operator ==(ChunkedServiceRequestMessageData a, ChunkedServiceRequestMessageData b)
        {
            if ((object)a == null || ((object)b) == null)
                return Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(ChunkedServiceRequestMessageData a, ChunkedServiceRequestMessageData b)
        {
            if (a == null || b == null)
                return !Equals(a, b);

            return !(a.Equals(b));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ChunkedServiceRequestMessageData)) return false;
            return Equals((ChunkedServiceRequestMessageData)obj);
        }

        public override int GetHashCode()
        {
            return CorrelationId.GetHashCode()^Order.GetHashCode();
        }

        #endregion

        public static ChunkedServiceRequestMessageData FromDataRow(DataRow dataRow)
        {
            var result = new ChunkedServiceRequestMessageData
            {
                Id = Convert.ToInt32(dataRow[GetDbFieldName("Id")]),
                ComponentOwner = (Guid)dataRow[GetDbFieldName("ComponentOwner")],
                Version = (long)dataRow[GetDbFieldName("Version")],
                Operation = (Guid)dataRow[GetDbFieldName("Operation")],
                Data = new List<byte>((byte[])dataRow[GetDbFieldName("Data")]),
                CorrelationId = (Guid)dataRow[GetDbFieldName("CorrelationId")],
                Order = Convert.ToInt32(dataRow[GetDbFieldName("Order")]),
                Eof = (bool)dataRow[GetDbFieldName("Eof")]
            };
            return result;
        }

        protected internal static string GetDbFieldName(string fieldName)
        {
            return String.Format("{0}_{1}", TableName, fieldName);
        }


        public static ChunkedServiceRequestMessageData NewFromExisting(ChunkedServiceRequestMessageData source)
        {
            var result = new ChunkedServiceRequestMessageData()
            {
                ComponentOwner = source.ComponentOwner,
                Version = source.Version,
                CorrelationId=source.CorrelationId,
                Data = source.Data,
                Eof=source.Eof,
                Operation = source.Operation,
                Order=source.Order

                
            };
            return result;
        }
    }
}
