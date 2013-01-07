// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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