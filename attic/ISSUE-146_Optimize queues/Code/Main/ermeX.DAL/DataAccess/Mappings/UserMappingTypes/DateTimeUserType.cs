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