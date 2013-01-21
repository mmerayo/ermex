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
using System.Linq;

namespace ermeX
{
    //TODO: STORE IN DB
    /// <summary>
    /// Decorates the interface definition to be used as an ermeX Service
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceContractAttribute : Attribute
    {
        /// <summary>
        /// cctor
        /// </summary>
        /// <param name="guidServiceIdentifier">Unique identifier of the service in the ermeX network</param>
        public ServiceContractAttribute(string guidServiceIdentifier) : this(guidServiceIdentifier, false)
        {
        }

        internal ServiceContractAttribute(string guidServiceIdentifier, bool isSystemService)
        {
            if (string.IsNullOrEmpty(guidServiceIdentifier))
                throw new ArgumentException("The serviceIdentifier is required");
            ServiceIdentifier = new Guid(guidServiceIdentifier);
            IsSystemService = isSystemService;
        }

        /// <summary>
        /// Gets the service identifier
        /// </summary>
        internal Guid ServiceIdentifier { get; private set; }

        internal bool IsSystemService { get; private set; }

        internal static bool IsDefinedIn(Type interfaceType)
        {
            return interfaceType.GetCustomAttributes(typeof (ServiceContractAttribute), true).SingleOrDefault() != null;
        }

        internal static ServiceContractAttribute GetFromType(Type interfaceType)
        {
            return
                (ServiceContractAttribute)
                interfaceType.GetCustomAttributes(typeof (ServiceContractAttribute), true).Single();
        }

        internal static bool GetIsSystemService(Type interfaceType)
        {
            if (!IsDefined(interfaceType, typeof (ServiceContractAttribute)))
                throw new InvalidOperationException(
                    "The service interface type must be decorated with [ServiceContract]");

            var serviceContractAttribute =
                interfaceType.GetCustomAttributes(typeof (ServiceContractAttribute), true).Single() as
                ServiceContractAttribute;
            return serviceContractAttribute.IsSystemService;
        }
    }
}