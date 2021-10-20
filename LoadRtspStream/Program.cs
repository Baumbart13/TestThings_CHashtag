using System;
using System.IO;
using System.Threading;
using Nager.VideoStream;
using NetworkConstants;
using static NetworkConstants.Raspberry;

namespace LoadRtspStream
{
    class Program
    {
        private static string startString = $"raspivid -w {VID_WIDTH} -h {VID_HEIGHT} -fps {VID_FPS}" +
                                            " -o - -t 0 -n | cvlc -vvv stream:///dev/stdin --sout '#rtp{sdp=rtsp://:" +
                                            $"{Address.Port}" +
                                            "/}' :demux=h264";

        private static string receiveString = $"rtsp://{Address.Address.MapToIPv4()}:{Address.Port}/";
        
        static void Main(string[] args)
        {
            Console.WriteLine($"startString:\"{startString}\"");
            Console.WriteLine($"receiveString:\"{receiveString}\"");
            
            var inputSource = new StreamInputSource("rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mov");

            var cancellationTokenSource = new CancellationTokenSource();

            var client = new VideoStreamClient("ffmpeg");
            client.NewImageReceived += NewImageReceived;
            var task = client.StartFrameReaderAsync(inputSource, OutputImageFormat.Png, cancellationTokenSource.Token);

//wait for exit
            Console.WriteLine("Wait for exit");
            Console.ReadLine();

            client.NewImageReceived -= NewImageReceived;
        }
        
        static void NewImageReceived(byte[] imageData)
        {
            File.WriteAllBytes("SimpleFile.bmp", imageData);
        }
    }
}