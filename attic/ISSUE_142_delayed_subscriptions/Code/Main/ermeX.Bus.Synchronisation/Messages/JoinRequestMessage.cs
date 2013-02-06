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

namespace ermeX.Bus.Synchronisation.Messages
{
    internal sealed class JoinRequestMessage : DialogMessageBase
    {
        //public JoinRequestMessage():base() //TODO: resolve restriction on deserializer when there is not an empty constructor
        //{}
        public JoinRequestMessage(Guid sourceComponentId, string sourceIp, int sourcePort)
            : base(sourceComponentId)
        {
            SourceIp = sourceIp;
            SourcePort = sourcePort;
        }

        public string SourceIp { get; private set; }
        public int SourcePort { get; set; }
    }
}