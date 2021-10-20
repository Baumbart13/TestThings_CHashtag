using System.Net;
using System.Net.Sockets;

namespace LoadTcpStream
{
    public class TcpListener
    {
        public TcpClient Client;

        public TcpListener(IPEndPoint streamSrc)
        {
            Client = new TcpClient(streamSrc);
        }
    }
}