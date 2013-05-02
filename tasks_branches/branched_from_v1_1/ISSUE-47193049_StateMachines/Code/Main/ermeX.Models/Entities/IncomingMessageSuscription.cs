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
using ermeX.Models.Base;

namespace ermeX.Models.Entities
{
    //listener suscriptions
    internal class IncomingMessageSuscription : ModelBase, IEquatable<IncomingMessageSuscription>
    {
        internal const string TableName = "IncomingMessageSuscriptions";

        public virtual string BizMessageFullTypeName { get; set; }

        public virtual Guid SuscriptionHandlerId { get; set; }

        public virtual DateTime DateLastUpdateUtc { get; set; }

        public virtual string HandlerType { get; set; }

        internal static string GetDbFieldName(string fieldName)
        {
            return string.Format("{0}_{1}", TableName, fieldName);
        }

        public static IncomingMessageSuscription FromDataRow(DataRow dataRow)
        {
            var result = new IncomingMessageSuscription
                             {
                                 Id = Convert.ToInt32( dataRow[GetDbFieldName("Id")]),
                                 Version = (long) dataRow[GetDbFieldName("Version")],
                                 ComponentOwner = (Guid) dataRow[GetDbFieldName("ComponentOwner")],
                                 SuscriptionHandlerId = (Guid) dataRow[GetDbFieldName("SuscriptionHandlerId")],
                                 BizMessageFullTypeName = dataRow[GetDbFieldName("BizMessageFullTypeName")].ToString(),
                                 DateLastUpdateUtc = new DateTime((long) dataRow[GetDbFieldName("DateLastUpdateUtc")]),
                                 HandlerType = (string) dataRow[GetDbFieldName("HandlerType")]
                             };
            return result;
        }


        #region Equatable

        public virtual bool Equals(IncomingMessageSuscription other)
        {
            if (other == null)
                return false;

            var result = BizMessageFullTypeName == other.BizMessageFullTypeName &&
                         SuscriptionHandlerId == other.SuscriptionHandlerId && Version == other.Version &&
                         HandlerType == other.HandlerType;

#if !NEED_FIX_MILLISECONDS
            result = result && DateLastUpdateUtc == other.DateLastUpdateUtc;
#endif
            return result;
        }

        public static bool operator ==(IncomingMessageSuscription a, IncomingMessageSuscription b)
        {
            if ((object) a == null || ((object) b) == null)
                return Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(IncomingMessageSuscription a, IncomingMessageSuscription b)
        {
            if (a == null || b == null)
                return !Equals(a, b);

            return !(a.Equals(b));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (IncomingMessageSuscription)) return false;
            return Equals((IncomingMessageSuscription) obj);
        }

        public override int GetHashCode()
        {
            return BizMessageFullTypeName.GetHashCode();
        }

        #endregion
    }
}