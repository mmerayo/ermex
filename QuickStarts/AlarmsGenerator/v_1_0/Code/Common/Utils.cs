using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Common
{
    public static class Utils
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
