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

namespace ermeX.Models
{
    internal class OutgoingMessageSuscription : ModelBase, IEquatable<OutgoingMessageSuscription>
    {

        public OutgoingMessageSuscription(IncomingMessageSuscriptionInfo suscriptionInfo, AppComponentInfo suscriberComponentId,
                                          AppComponentInfo localComponentId)
        {
            Component = suscriberComponentId;
            OwnedBy = localComponentId;
            BizMessageFullTypeName = suscriptionInfo.BizMessageFullTypeName;
            DateLastUpdateUtc = suscriptionInfo.DateLastUpdateUtc;
        }

        public OutgoingMessageSuscription()
        {
        }

        public  string BizMessageFullTypeName { get; set; }

        public  AppComponentInfo Component { get; set; }

        public  DateTime DateLastUpdateUtc { get; set; }

       

        #region Equatable

        public  bool Equals(OutgoingMessageSuscription other)
        {
            if (other == null)
                return false;

            var result = Component.ToString() == other.Component.ToString() &&
                         BizMessageFullTypeName == other.BizMessageFullTypeName && Version == other.Version;
#if !NEED_FIX_MILLISECONDS
            result = result && DateLastUpdateUtc == other.DateLastUpdateUtc;
#endif
            return result;
        }

        public static bool operator ==(OutgoingMessageSuscription a, OutgoingMessageSuscription b)
        {
            if ((object) a == null || ((object) b) == null)
                return Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(OutgoingMessageSuscription a, OutgoingMessageSuscription b)
        {
            if (a == null || b == null)
                return !Equals(a, b);

            return !(a.Equals(b));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (OutgoingMessageSuscription)) return false;
            return Equals((OutgoingMessageSuscription) obj);
        }

        public override int GetHashCode()
        {
            return Component.GetHashCode();
        }

        #endregion

    }
}