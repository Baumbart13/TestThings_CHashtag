﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using ModifyColors;
using NetMQ;
using NetMQ.Sockets;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static NetworkConstants.Constants;

static class Program
{
    public static void Main()
    {
        const string msg = "Hello";
        Console.WriteLine("Connecting to hello world server…");
        using(var requester = new NetMQ.Sockets.RequestSocket())
        {
            requester.Connect($"tcp://{Localhost}:{Port}");

            int requestNumber;
            //for (requestNumber = 0; requestNumber != 10; requestNumber++)
            {
                // Let's try this shit and send an image as a string
                Image<Rgba32> img = null;
                using(var inStream = File.Open(@"D:\Benutzer\Baumbart13\JetBrains\Rider\TestThings_CHashtag\ModifyColors\res\Audi.png", FileMode.Open))
                {
                    img = Image.Load(inStream).CloneAs<Rgba32>();
                    //img.Mutate(i => i.Flip(FlipMode.Horizontal));
                    //img.Mutate(i => i.Rotate(RotateMode.Rotate270));
                }
                Console.WriteLine("Encoding it to StringBuilder");
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
                        var p = img[x,y];
                        sb.Append(p.R);
                        sb.Append(';');
                        sb.Append(p.G);
                        sb.Append(';');
                        sb.Append(p.B);
                        sb.Append(';');
                    }
                }
                img.Dispose();

                Console.WriteLine("Sending to server");
                requester.SendFrame(sb.ToString()); // as in a string decoded
                //requester.SendMultipartMessage(netMqMsg); // TODO: Create a NetMQMessage
                Console.WriteLine("Waiting for reply");
                var str = requester.ReceiveFrameString();
                Console.WriteLine($"[{DateTime.Now}]: Server is saying: \"{str}\"");
            }
        }
    }
}