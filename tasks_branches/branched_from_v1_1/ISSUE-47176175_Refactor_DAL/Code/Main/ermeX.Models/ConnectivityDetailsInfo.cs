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
    //TODO: MODELBASE AND THIS CASES SHOULD BE MUCH LIGHTER
    internal class ConnectivityDetailsInfo : ModelBase, IEquatable<ConnectivityDetailsInfo>
    {
      

        public  string Ip { get; set; }

        public  bool IsLocal //TODO: REMOVE THIS FIELD
        { get; set; }


        public  int Port { get; set; }
        //TODO: rename to RemoteComponentId
        public  Guid ServerId { get; set; }

        #region Equatable

        //TODO: refactor to base

        public  bool Equals(ConnectivityDetailsInfo other)
        {
            if (other == null)
                return false;

            return Ip == other.Ip && Port == other.Port && Version == other.Version;
        }

        public static bool operator ==(ConnectivityDetailsInfo a, ConnectivityDetailsInfo b)
        {
            if ((object) a == null || ((object) b) == null)
                return Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(ConnectivityDetailsInfo a, ConnectivityDetailsInfo b)
        {
            if (a == null || b == null)
                return !Equals(a, b);

            return !(a.Equals(b));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ConnectivityDetailsInfo)) return false;
            return Equals((ConnectivityDetailsInfo) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion

		
    }
}