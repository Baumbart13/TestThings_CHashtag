using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

static class Program
{
    public static void Main()
    {        
        using (var responder = new ServerSocket())
        {
            responder.Bind("tcp://*:5555");

            while (true)
            {
                var str = responder.ReceiveBytesAsync();
                Console.WriteLine($"[{DateTime.Now.Date}][{DateTime.Now.TimeOfDay}]: Received \"{str}\" from Client");
                Thread.Sleep(1000);  //  Do some 'work'
            }
        }
    }
}