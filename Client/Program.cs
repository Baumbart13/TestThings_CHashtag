using System;
using NetMQ;
using NetMQ.Sockets;
using static NetworkConstants.Constants;

static class Program
{
    public static void Main()
    {
        const string msg = "Hello";
        Console.WriteLine("Connecting to hello world server…");
        using(var requester = new NetMQ.Sockets.RequestSocket())
        {
            requester.Connect($"tcp://{TestIp}:{Port}");

            int requestNumber;
            for (requestNumber = 0; requestNumber != 10; requestNumber++)
            {
                Console.WriteLine($"[{DateTime.Now.Date}][{DateTime.Now.TimeOfDay}]: Sending \"{msg}\" with request number \"{requestNumber}\" to Server...");
                requester.SendFrame(msg);
                string str = requester.ReceiveFrameString();
                Console.WriteLine($"[{DateTime.Now.Date}][{DateTime.Now.TimeOfDay}]: Received \"{str}\" with request number \"{requestNumber}\" from Server");
            }
        }
    }    
}