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
using System.Collections.Generic;
using System.Linq;

namespace ermeX.Bus.Interfaces.Attributes
{
    //TODO: ATTRIBUTE FOR public services class and methods

    [AttributeUsage(AttributeTargets.Method)]
    //TODO: CREATE PROPERTY OPERATION NAME AND USE IT INSTEAD OF THE methodinfoname i the whole app validating that is unique per definition
    public class ServiceOperationAttribute : Attribute
    {
        public ServiceOperationAttribute(string operationIdentifier)
        {
            if (string.IsNullOrEmpty(operationIdentifier))
                throw new ArgumentException("The operationIdentifier is required");
            OperationIdentifier = new Guid(operationIdentifier);
        }

        internal Guid OperationIdentifier { get; private set; }

        internal static Guid GetOperationIdentifier(object source, string methodName)
        {
            return GetOperationIdentifier(source.GetType(), methodName);
        }

        internal static Guid GetOperationIdentifier(Type type, string methodName)
        {
            if (!IsDefined(type, typeof (ServiceContractAttribute)))
                throw new InvalidOperationException(
                    "The service interface type must be decorated with [ServiceContract]");

            var methodInfos = type.GetMethods().Where(x => x.Name == methodName);


            Guid result = Guid.Empty;
            bool found = false;
            foreach (var methodInfo in methodInfos)
            {
                if (found)
                    throw new InvalidOperationException(
                        "There are two methods with the same name that are service operations");
                if (IsDefined(methodInfo, typeof (ServiceOperationAttribute)))
                {
                    var instance =
                        methodInfo.GetCustomAttributes(typeof (ServiceOperationAttribute), true).Single() as
                        ServiceOperationAttribute;
                    result = instance.OperationIdentifier;
                    found = true;
                }
            }
            if (!found)
                throw new ArgumentException("Not found " + methodName + " decorated with [ServiceOperation]");

            return result;
        }

        internal static IEnumerable<ServiceOperationAttribute> GetOperations(Type type)
        {
            if (!IsDefined(type, typeof (ServiceContractAttribute)))
                throw new InvalidOperationException(
                    "The service interface type must be decorated with [ServiceContract]");

            var result = new List<ServiceOperationAttribute>();

            var methodInfos = type.GetMethods();

            foreach (var methodInfo in methodInfos)
            {
                if (IsDefined(methodInfo, typeof (ServiceOperationAttribute)))
                {
                    result.Add(
                        methodInfo.GetCustomAttributes(typeof (ServiceOperationAttribute), true).Single() as
                        ServiceOperationAttribute);
                }
            }
            return result;
        }

        public static IEnumerable<string> GetServiceNames(Type typeService)
        {
            if (!IsDefined(typeService, typeof (ServiceContractAttribute)))
                throw new InvalidOperationException(
                    "The service interface type must be decorated with [ServiceContract]");

            var result = new List<string>();

            var methodInfos = typeService.GetMethods();

            foreach (var methodInfo in methodInfos)
                if (IsDefined(methodInfo, typeof (ServiceOperationAttribute)))
                    result.Add(methodInfo.Name);

            return result;
        }
    }
}