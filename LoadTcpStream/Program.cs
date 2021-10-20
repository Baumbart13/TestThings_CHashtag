using System;
using System.Net;

namespace LoadTcpStream
{
    class Program
    {
        private static TcpListener _tcpListener = new TcpListener(
            new IPEndPoint(IPAddress.Parse("192.168.68.116"), 5000)
            );
        
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}