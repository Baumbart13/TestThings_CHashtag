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
            new KeyValuePair<string, string>("user", Username),
            new KeyValuePair<string, string>("password", Password)
        });
        
        var img = new Image<Rgba32>(1, 1);
        try
        {
            var response = RestHandler.Get(Url, param);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // image comes directly as JPEG
                var decoder = new JpegDecoder();
                using var stream = response.GetResponseStream();
                Console.WriteLine("Reading and decoding the JPEG-Array into a image");
                decoder.Decode<Rgba32>(Configuration.Default, stream, CancellationToken.None);

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