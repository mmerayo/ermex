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
    internal class ServiceDetails : ModelBase, IEquatable<ServiceDetails>
    {
        internal const string TableName = "ServicesDetails"; 

        public virtual Guid OperationIdentifier { get; set; }

        public virtual string ServiceImplementationTypeName { get; set; }

        public virtual string ServiceImplementationMethodName { get; set; }

        public virtual string ServiceInterfaceTypeName { get; set; }

        public virtual bool IsSystemService { get; set; }


        public virtual Guid Publisher { get; set; }

        #region IEquatable

        public virtual bool Equals(ServiceDetails other)
        {
            if (other == null)
                return false;

            return OperationIdentifier == other.OperationIdentifier &&
                   ServiceInterfaceTypeName == other.ServiceInterfaceTypeName &&
                   ServiceImplementationTypeName == other.ServiceImplementationTypeName &&
                   ServiceImplementationMethodName == other.ServiceImplementationMethodName && Version == other.Version;
        }

        public static bool operator ==(ServiceDetails a, ServiceDetails b)
        {
            if ((object) a == null || ((object) b) == null)
                return Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(ServiceDetails a, ServiceDetails b)
        {
            if (a == null || b == null)
                return !Equals(a, b);

            return !(a.Equals(b));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ServiceDetails)) return false;
            return Equals((ServiceDetails) obj);
        }

        public override int GetHashCode()
        {
            return OperationIdentifier.GetHashCode();
        }

        #endregion

        protected internal static string GetDbFieldName(string fieldName)
        {
            return String.Format("{0}_{1}", TableName, fieldName);
        }

        public static ServiceDetails FromDataRow(DataRow dataRow)
        {
            var result = new ServiceDetails
                             {
                                 Id = Convert.ToInt32( dataRow[GetDbFieldName("Id")]),
                                 ComponentOwner = (Guid) dataRow[GetDbFieldName("ComponentOwner")],
                                 Publisher = (Guid) dataRow[GetDbFieldName("Publisher")],
                                 OperationIdentifier = (Guid) dataRow[GetDbFieldName("OperationIdentifier")],
                                 ServiceImplementationMethodName =
                                     (string) dataRow[GetDbFieldName("ServiceImplementationMethodName")],
                                 ServiceImplementationTypeName =
                                     (string) dataRow[GetDbFieldName("ServiceImplementationTypeName")],
                                 ServiceInterfaceTypeName = (string) dataRow[GetDbFieldName("ServiceInterfaceTypeName")],
                                 IsSystemService = (bool) dataRow[GetDbFieldName("IsSystemService")],
                                 Version = (long) dataRow[GetDbFieldName("Version")],
                             };
            return result;
        }
    }
}