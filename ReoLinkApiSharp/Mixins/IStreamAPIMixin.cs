using System.Collections;
using System.Collections.Immutable;
using System.Net;
using System.Text;
using Emgu.CV;
using ReoLinkApiSharp.Handlers;
using ReoLinkApiSharp.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace ReoLinkApiSharp.Mixins;

/// <summary>
/// API calls for opening a video stream or capturing an image from the camera.
/// </summary>
public interface IStreamAPIMixin
{
    private static Random random = new Random();

    public IEnumerable<Mat> OpenVideoStream()
    {
        var rtspClient = new RtspClient(IpAddress, Username, Password, Port, Profile);
        return rtspClient.OpenStream();
    }

    private static string BrowserCachingPreventionString(int length)
    {
        var sb = new StringBuilder(length);
        char ch;
        for (var i = 0; i < length; ++i)
        {
            ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
            sb.Append(ch);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets a "snap" of the current camera video data and returns an Image
    /// </summary>
    /// <param name="timeout">Request timeout to camera in seconds</param>
    /// <returns>Image may be size of 1x1 if an error occurred</returns>
    public Image<Rgba32> GetSnap(int timeout = 3)
    {
        Console.WriteLine("Getting Snap");
        var param = new Dictionary<string, string>(new []{
            new KeyValuePair<string, string>("cmd", "Snap"),
            new KeyValuePair<string, string>("channel", "0"),
            new KeyValuePair<string, string>("rs", BrowserCachingPreventionString(10)),
            new KeyValuePair<string, string>("token", token)
        });
        var img = new Image<Rgba32>(1, 1);
        try
        {
            var response = RestHandler.Get(Url, param);
            if (response.IsSuccessStatusCode)
            {
                var dict = response.Content.Headers.ToImmutableDictionary();
                foreach (var (key, value) in dict)
                {
                    Console.WriteLine($"{key}:{value}");
                }
                Console.WriteLine("=====\nContent:\n=======");
                var receivedStr = response.Content.ReadAsStringAsync().Result;
                var str = receivedStr;
                File.Create()
                File.WriteAllText(@"C:\Users\Baumbart13\ResponseContent_Python.txt", str);
                Console.WriteLine($"Length of content: {str.Length}");
                Console.WriteLine($"\n\n=====\n{str}\n=====");

                Environment.Exit(-1);
                // image comes directly as JPEG theoretically
                using var stream = response.Content.ReadAsStream();
                var decoder = new JpegDecoder();
                var width = stream.ReadByte();
                width = (width << 8) | stream.ReadByte();
                var height = stream.ReadByte();
                height = (height << 8) | stream.ReadByte();
                img = new Image<Rgba32>(width, height);
                var pixels = new ArrayList(1000);
                while (stream.CanRead)
                {
                    if (pixels.Count > 171648)
                    {
                        Console.WriteLine("Content-Length cannot be more");
                        Environment.Exit(-1);
                    }
                    pixels.Add(stream.ReadByte());
                }

                return img;
                img = decoder.Decode<Rgba32>(Configuration.Default, stream);
                return img;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not get Image data\n{e}");
            return img;
        }
        return img;
    }

    public IPAddress IpAddress { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public Profile Profile { get; set; }
    public int Port { get; set; }
    public string Url { get; set; }
}