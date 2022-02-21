using System;
using ModifyColors;
using NetMQ;
using NetMQ.Sockets;
using Network;
using NetworkConstants;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

static class Program
{
    private const string FILENAME = "NetMQ.png";
    private const string SAVE_PATH = $@"C:\Users\Baumbart13\Pictures\{FILENAME}";

    public static void Main()
    {
        Console.WriteLine("Creating server instance");
        using var responder = new ResponseSocket();
        Constants.WriteLocalIpToFile();
        responder.Bind($"tcp://*:{Constants.ServerCredentials.Port}");
        Console.WriteLine($"Bound address to TCP on Port {Constants.ServerCredentials.Port}");


        while (true)
        {
            Console.WriteLine("Waiting for message");
            
            //VersionStrBuilder(responder);
            VersionArtemisMessage(responder);
        }
    }

    private static void VersionArtemisMessage(ResponseSocket responder)
    {
        var netMqMsg = responder.ReceiveMultipartMessage(3);
        var artemisMsg = Network.Message.FromNetMqMessage(netMqMsg);
        // Decode the image and save it
        Console.WriteLine($"[{DateTime.Now} {DateTime.Now}]: Received message from Client");
        //  Decode it
        var responseMsg = $"Received Image at [{DateTime.Now} {DateTime.Now.Millisecond}]\n";

        var img = artemisMsg.ReadImage();
        var colFormat = ImageManipulator.GetColorFormat(img) == ColorFormat.Rgba32 ? "Rgba32" : "Grayscale";
        Console.WriteLine($"ColorFormat is {colFormat}");
        var Width = img.Width;
        var Height = img.Height;
        Console.WriteLine($"Image has a size of {Width}x{Height} pixels");
        responseMsg += $"Decoded Image at [{DateTime.Now}]\n";
            
        // save it
        img.SaveAsPng(SAVE_PATH);
        responseMsg += $"Saved Image at [{DateTime.Now}] as \"{SAVE_PATH}\"";

        responder.SendFrame(responseMsg);
    }

    private static void VersionStrBuilder(ResponseSocket responder)
    {
        var str = responder.ReceiveFrameString();
        var strSeparated = str.Split(';');
        // Decode the image and save it
        Console.WriteLine($"[{DateTime.Now} {DateTime.Now}]: Received message from Client");
        //  Decode it
        var responseMsg = $"Received Image at [{DateTime.Now} {DateTime.Now.Millisecond}]\n";

        var currArgFromMsg = 0;
        var colFormat = (ColorFormat)Convert.ToInt32(strSeparated[currArgFromMsg++]);
        Console.WriteLine($"ColorFormat is {nameof(colFormat)}");
        var Width = Convert.ToInt32(strSeparated[currArgFromMsg++]);
        var Height = Convert.ToInt32(strSeparated[currArgFromMsg++]);
        Console.WriteLine($"Image has a size of {Width}x{Height} pixels");

        using (var img = new Image<Rgba32>(Width, Height))
        {
            try
            {
                for (var x = 0; x < Width; ++x)
                {
                    for (var y = 0; y < Height; ++y)
                    {
                        var r = Convert.ToByte(strSeparated[currArgFromMsg++]);
                        var g = Convert.ToByte(strSeparated[currArgFromMsg++]);
                        var b = Convert.ToByte(strSeparated[currArgFromMsg++]);
                        var newP = new Rgba32(r, g, b);
                        img[x, y] = newP;
                    }
                }

                responseMsg += $"Decoded Image at [{DateTime.Now}]\n";

                // save it
                img.SaveAsPng(SAVE_PATH);
                responseMsg += $"Saved Image at [{DateTime.Now}] as \"{FILENAME}\"";
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                responder.SendFrame(e.Message);
                img.Dispose();
                return;
            }
        }

        responder.SendFrame(responseMsg);
    }
}