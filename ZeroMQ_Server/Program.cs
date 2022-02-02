using ZeroMQ;

namespace ZeroMQ_Server
{
    public class Program
    {
        public static void HelloWorldServer(string arg)
        {
            if(arg == null || arg.Length < 1)
            {
                Console.WriteLine();
                Console.WriteLine("Usage: ./{0} HWServer [Name]", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine();
                Console.WriteLine("    Name   Your name. Default: World");
                Console.WriteLine();
                arg = "World";
            }

            var name = arg;

            // Create
            using (var context = new ZContext())
            using (var responder = new ZSocket(context, ZSocketType.REP))
            {
                // Bind
                responder.Bind("tcp://*:5555");

                while (true)
                {
                    // Receive
                    using (ZFrame request = responder.ReceiveFrame())
                    {
                        var req = request.ReadString();
                        Console.WriteLine("Received {0}", req);

                        // Do some work
                        Thread.Sleep(1);

                        // Send
                        responder.Send(new ZFrame($"\"{name}\" answered to message \"{req}\""));
                    }
                }
            }
        }
        
        public static void Main(string[] args)
        {
            HelloWorldServer(args[0]);
            Console.WriteLine("Exiting program");
        }
    }
}