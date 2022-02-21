using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace NetworkConstants
{
    public class Constants
    {
        public class ServerCredentials
        {
            public IPAddress IpAddress { get; private set; } = IPAddress.None;
            public string Username { get; private set; } = "";
            public string Password { get; private set; } = "";
            public const int Port = 42555;
            public const string TestServerIp = "142.132.224.12";
            private ServerCredentials(){}

            public static ServerCredentials FromConfigFile(string filePath)
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("Config-file for Server not found!");
                }
                var fileContents = File.ReadAllLines(filePath);

                var cred = new ServerCredentials();
                foreach (var line in fileContents)
                {
                    if (line.StartsWith("ip")){
                        cred.IpAddress = IPAddress.Parse(line.Split(':')[1]);
                    }else if (line.StartsWith("username")){
                        cred.Username = line.Split(':')[1];
                    }else if (line.StartsWith("password"))
                    {
                        cred.Password = line.Split(':')[1];
                    }else if (line.Equals(""))
                    {
                        break;
                    }
                }

                return cred;
            }
        }
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

        public static void WriteLocalIpToFile(string filepath = "serverIp.config")
        {
            var ip = GetLocalIPAddress();
            if (File.Exists(filepath))
            {
                AddIpToFile(filepath, ip);
                return;
            }

            File.WriteAllText(filepath, $"ip:{ip}");
        }

        private static void AddIpToFile(string filepath, string ip)
        {
            var lines = File.ReadAllLines(filepath);
            foreach (var line in lines)
            {
                if (line.StartsWith("ip"))
                {
                    return;
                }
            }

            using var writer = File.AppendText(filepath);
            writer.WriteLine($"ip:{ip}");
        }

        public const string Localhost = "localhost";
        public const string LocalhostAsIp = "127.0.0.1";
    }
}