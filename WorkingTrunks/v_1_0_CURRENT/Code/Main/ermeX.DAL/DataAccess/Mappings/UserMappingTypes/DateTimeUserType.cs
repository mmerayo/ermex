// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Data;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace ermeX.DAL.DataAccess.Mappings.UserMappingTypes
{
    public class DateTimeUserType : IUserType
    {
        #region IUserType Members

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            var ticks = (long) NHibernateUtil.Int64.NullSafeGet(rs, names[0]);
            return new DateTime(ticks);
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            var parameter = (IDataParameter) cmd.Parameters[index];
            if (value == null)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = ((DateTime) value).Ticks;
        }

        public Type ReturnedType
        {
            get { return typeof (DateTime); }
        }

        public SqlType[] SqlTypes
        {
            get { return new[] {SqlTypeFactory.Int64}; }
        }

        public new virtual bool Equals(object x, object y)
        {
            return object.Equals(x, y);
        }

        public virtual int GetHashCode(object x)
        {
            return (x == null) ? 0 : x.GetHashCode();
        }

        public bool IsMutable
        {
            get { return false; }
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        #endregion
    }
}