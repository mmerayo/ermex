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
    internal class ServiceDetails : ModelBase, IEquatable<ServiceDetails>
    {
        public  Guid OperationIdentifier { get; set; }

        public  string ServiceImplementationTypeName { get; set; }

        public  string ServiceImplementationMethodName { get; set; }

        public  string ServiceInterfaceTypeName { get; set; }

        public  bool IsSystemService { get; set; }


        public  Guid Publisher { get; set; }

        #region IEquatable

        public  bool Equals(ServiceDetails other)
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

      

    }
}