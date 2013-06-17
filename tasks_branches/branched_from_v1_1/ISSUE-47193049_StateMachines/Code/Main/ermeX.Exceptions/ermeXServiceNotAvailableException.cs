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
using System.Text;

namespace ermeX.Exceptions
{
	[Serializable]
    public class ermeXServiceNotAvailableException : ermeXException
    {
        public ermeXServiceNotAvailableException(string interfaceName, string methodName)
            : this(interfaceName, methodName, null){    }
        public ermeXServiceNotAvailableException(string interfaceName, string methodName,Exception innerException) : this(interfaceName, methodName,innerException, null) { }
        public ermeXServiceNotAvailableException(string interfaceName, string methodName,Exception innerException, Guid? destinationComponent) 
            : base(string.Format("Component:{2} Service: {0}.{1} is not defined locally or the current assembly has not referenced it", interfaceName, methodName,
            destinationComponent.HasValue ? destinationComponent.Value.ToString() : "Unspecified"),innerException) { }
    }
}
