﻿// /*---------------------------------------------------------------------------------------*/
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
using System.Net;
using System.Net.Sockets;

namespace Common.Other
{
    public static class Networking
    {
        public static string GetLocalhostIp(AddressFamily addressFamily)
        {
            IPHostEntry host;
            string result = null;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == addressFamily)
                {
                    result = ip.ToString();
                }
            }
            return result;
        }

        public static string GetLocalhostIp()
        {
            return GetLocalhostIp(AddressFamily.InterNetwork);
        }
    }
}