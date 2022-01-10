using System;
using NetMQ;
using NetMQ.Sockets;

static class Program
{
    public static void Main()
    {
        const string msg = "Hello";
        Console.WriteLine("Connecting to hello world server…");
        using(var requester = new RequestSocket())
        {
            requester.Connect("tcp://localhost:5555");

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