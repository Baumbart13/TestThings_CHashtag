using System;
using System.IO;
using System.Text;
using System.Text.Json;
using ModifyColors;
using NetMQ;
using NetMQ.Sockets;
using Network;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static NetworkConstants.Constants;
using Configuration = Network.Utils.Configuration;

static class Program
{
    private const string LOAD_PATH = @"C:\Users\Baumbart13\Pictures\p1.png";
    public static void Main()
    {
        var configPath = "";
        Console.WriteLine("Reading config-file");
        var config = Configuration.ReadFromFile();
        Console.WriteLine("\n\n");
        var configJson = config.ToJsonString();
        Console.WriteLine("Configuration as text:\n" +
                          "======================" +
                          $"{configJson}");
        return;
        
        Console.WriteLine("Connecting to hello world server…");
        using (var requester = new NetMQ.Sockets.RequestSocket())
        {
            requester.Connect($"tcp://{ServerCredentials.TestServerIp}:{ServerCredentials.Port}");
            Console.WriteLine($"Connected to {ServerCredentials.TestServerIp}:{ServerCredentials.Port}");

            int requestNumber;
            //for (requestNumber = 0; requestNumber != 10; requestNumber++)
            {
                // Let's try this shit and send an image as a string
                Console.WriteLine("Loading the image");
                Image<Rgba32> img = null;
                using (var inStream =
                       File.Open(LOAD_PATH, FileMode.Open))
                {
                    img = Image.Load(inStream).CloneAs<Rgba32>();
                }
                
                //VersionStrBuilder(requester, img);
                VersionArtemisMessage(requester, img);
                img.Dispose();
            }
        }
    }

    private static void VersionArtemisMessage(RequestSocket requester, Image<Rgba32> img)
    {
        Console.WriteLine("Encoding it as ArtemisMessage");
        var msg = new Network.Message(MessageType.Image);
        msg.AddImage(img);

        img.Dispose();

        Console.WriteLine("Sending to server");
        requester.SendMultipartMessage(msg.ToNetMqMessage());
        Console.WriteLine("Waiting for reply");
        var str = requester.ReceiveFrameString();
        Console.WriteLine($"[{DateTime.Now}]: Server is saying: \"{str}\"");
    }

    private static void VersionStrBuilder(RequestSocket requester, Image<Rgba32> img)
    {

        Console.WriteLine("Encoding it via StringBuilder");
        var sb = new StringBuilder();
        sb.Append((int)ColorFormat.Rgba32);
        sb.Append(';');
        sb.Append(img.Width);
        sb.Append(';');
        sb.Append(img.Height);
        sb.Append(';');
        for (var x = 0; x < img.Width; ++x)
        {
            for (var y = 0; y < img.Height; ++y)
            {
                var p = img[x, y];
                sb.Append(p.R);
                sb.Append(';');
                sb.Append(p.G);
                sb.Append(';');
                sb.Append(p.B);
                sb.Append(';');
            }
        }

        Console.WriteLine("Sending to server");
        requester.SendFrame(sb.ToString()); // as in a string decoded
        Console.WriteLine("Waiting for reply");
        var str = requester.ReceiveFrameString();
        Console.WriteLine($"[{DateTime.Now}]: Server is saying: \"{str}\"");
    }
}