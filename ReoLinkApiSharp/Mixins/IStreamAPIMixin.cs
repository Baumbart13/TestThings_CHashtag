using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Emgu.CV;
using Emgu.CV.Structure;
using ReoLinkApiSharp.Utils;
using RestSharp;
using SixLabors.ImageSharp;
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

    public Image<Rgba32> ConvertVideoStreamToImage(IEnumerable<Mat> streamData)
    {
        
        
        return new Image<Rgba32>(1, 1);
    }

    private static string CreateRandomString(int length)
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
        var body = new JsonArray(new JsonObject(new[]
        {
            new KeyValuePair<string, JsonNode?>("cmd", "Snap"),
            new KeyValuePair<string, JsonNode?>("channel", 0),
            new KeyValuePair<string, JsonNode?>("rs", CreateRandomString(10)),
            new KeyValuePair<string, JsonNode?>("user", Username),
            new KeyValuePair<string, JsonNode?>("password", Password)
        }));
        try
        {
            var httpWebRequest = HttpWebRequest.CreateHttp($"{Url}?cmd=Login&token=null");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = body.ToJsonString();
                streamWriter.Write(json);
            }

            var imageBytes = new List<int>(1920*1080);
            using (var httpResponse = httpWebRequest.GetResponse())
            {
                // TODO: Check for response-status; if 200, then open image and return the image
                using var streamReader = (httpResponse.GetResponseStream());
                int readByte;
                while((readByte = streamReader.ReadByte()) != -1)
                {
                    imageBytes.Add(readByte);
                }
            }
            
        }
        catch (Exception e)
        {
            Console.WriteLine("Could not get Image data");
            return new Image<Rgba32>(1, 1);
        }
        return new Image<Rgba32>(1, 1);
    }

    public IPAddress IpAddress { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public Profile Profile { get; set; }
    public int Port { get; set; }
    public string Url { get; set; }
}