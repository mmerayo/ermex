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
using ermeX.Entities.Entities;

namespace ermeX.Transport.Interfaces.Entities
{
    internal class ServerInfo
    {
        public ServerInfo()
        {
        }

        public ServerInfo(ConnectivityDetails details)
        {
            if (details == null) throw new ArgumentNullException("details");
            ServerId = details.ServerId;
            Ip = details.Ip;
            Port = details.Port;
            IsLocal = details.IsLocal;
        }

        public Guid ServerId { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public bool IsLocal { get; set; } //TODO:CHANGE THIS TO PROTOCOL 
    }
}