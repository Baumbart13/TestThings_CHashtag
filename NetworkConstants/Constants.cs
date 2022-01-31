using System;
using System.Net;
using System.Net.Sockets;

namespace NetworkConstants
{
    public class Constants
    {
        public const string TestIp = "192.168.68.111"; // TODO: This is just a test-ip!!!
        public const int Port = 42555;
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public const string Localhost = "localhost";
        public const string LocalhostAsIp = "127.0.0.1";
    }
}