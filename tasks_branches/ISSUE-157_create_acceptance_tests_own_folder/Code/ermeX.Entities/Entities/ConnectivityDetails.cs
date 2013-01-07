// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Data;
using ermeX.Entities.Base;

namespace ermeX.Entities.Entities
{
    //TODO: MODELBASE AND THIS CASES SHOULD BE MUCH LIGHTER
    internal class ConnectivityDetails : ModelBase, IEquatable<ConnectivityDetails>
    {
        public static string TableName
        {
            get { return "ConnectivityDetails"; }
        }

        public virtual string Ip { get; set; }

        public virtual bool IsLocal //TODO: REMOVE THIS FIELD
        { get; set; }


        public virtual int Port { get; set; }
        //TODO: rename to RemoteComponentId
        public virtual Guid ServerId { get; set; }

        public static ConnectivityDetails FromDataRow(DataRow dataRow)
        {
            var result = new ConnectivityDetails
                             {
                                 Id = Convert.ToInt32( dataRow[GetDbFieldName("Id")]),
                                 ComponentOwner = (Guid) dataRow[GetDbFieldName("ComponentOwner")],
                                 Ip = dataRow[GetDbFieldName("Ip")].ToString(),
                                 Version = (long) dataRow[GetDbFieldName("Version")],
                                 Port = Convert.ToInt32( dataRow[GetDbFieldName("Port")]),
                                 IsLocal = (bool) dataRow[GetDbFieldName("IsLocal")],
                                 ServerId = (Guid) dataRow[GetDbFieldName("ServerId")],
                             };
            return result;
        }

        protected internal static string GetDbFieldName(string fieldName) //TODO: REFACTOR
        {
            return String.Format("{0}_{1}", TableName, fieldName);
        }

        #region Equatable

        //TODO: refactor to base

        public virtual bool Equals(ConnectivityDetails other)
        {
            if (other == null)
                return false;

            return Ip == other.Ip && Port == other.Port && Version == other.Version;
        }

        public static bool operator ==(ConnectivityDetails a, ConnectivityDetails b)
        {
            if ((object) a == null || ((object) b) == null)
                return Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(ConnectivityDetails a, ConnectivityDetails b)
        {
            if (a == null || b == null)
                return !Equals(a, b);

            return !(a.Equals(b));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ConnectivityDetails)) return false;
            return Equals((ConnectivityDetails) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}