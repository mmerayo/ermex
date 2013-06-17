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

namespace ermeX.Exceptions
{
	[Serializable]
    public class ermeXUndefinedServiceException : ermeXException
    {
        public ermeXUndefinedServiceException(string interfaceName,string methodName):this(interfaceName,methodName,null) {}
        public ermeXUndefinedServiceException(string interfaceName,string methodName,Guid? destinationComponent)
            :base(string.Format("Component:{2} Service: {0}.{1} is not defined locally",interfaceName,methodName,destinationComponent.HasValue?destinationComponent.Value.ToString():"Unspecified"))
        {
            InterfaceName = interfaceName;
            MethodName = methodName;
            DestinationComponent = destinationComponent;
        }

        public string InterfaceName { get; private set; }
        public string MethodName { get; private set; }
        public Guid? DestinationComponent { get; private set; }
    }
}