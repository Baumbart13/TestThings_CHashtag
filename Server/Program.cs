using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

static class Program
{
    public static void Main()
    {        
        using (var responder = new ResponseSocket())
        {
            responder.Bind($"tcp://*:{NetworkConstants.Constants.Port}");

            while (true)
            {
                var str = responder.ReceiveFrameString();
                Console.WriteLine($"[{DateTime.Now.Date}][{DateTime.Now.TimeOfDay}]: Received \"{str}\" from Client");
                Thread.Sleep(1000);  //  Do some 'work'
                responder.SendFrame("World");
            }
        }
    }
}