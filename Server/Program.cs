using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ModifyColors;
using NetMQ;
using NetMQ.Sockets;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
                        img.SaveAsPng(@"C:\Users\Baumbart13\RiderProjects\TestThings\ModifyColors\res\NetMQ.png");
                        responseMsg += $"Saved Image at [{DateTime.Now}] as \"NetMQ.png\"";
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                        responder.SendFrame(e.Message);
                        img.Dispose();
                        continue;
                    }
                }

                responder.SendFrame(responseMsg);
            }
        }
    }
}