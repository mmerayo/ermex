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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace ermeX.Common
{
    [DebuggerStepThrough]
    public static class Networking
    {
        private static readonly object SyncLock = new object();

        public static bool PortIsBusy(ushort port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] endpoints = ipGlobalProperties.GetActiveTcpListeners();

            if (endpoints == null || endpoints.Length == 0) return false;
            return endpoints.Any(t => t.Port == port);
        }

        public static string GetLocalhostIp()
        {
            return GetLocalhostIp(AddressFamily.InterNetwork);
        }

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

        public static ushort GetFreePort(ushort bottomRange, ushort topRange)
        {
            var x = bottomRange;
            var y = topRange;
            if (bottomRange > topRange)
            {
                x = topRange;
                y = bottomRange;
            }

            for (var i = x; i <= y; i++)
            {
                if (!PortIsBusy(i))
                    return i;
            }

            throw new ApplicationException(
                string.Format("All the ports from {0} to {1} are busy. You may extend the range", x, y));
        }

        public static ushort GetFreePort(ushort bottomRange)
        {
            return GetFreePort(bottomRange, ushort.MaxValue);
        }
    }
}