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

    private static ParametersCollection CreateCommand(string user, string password)
    {
        var p = new ParametersCollection(new []
        {
            Parameter.CreateParameter("cmd", "Snap", ParameterType.GetOrPost),
            Parameter.CreateParameter("channel", 0, ParameterType.GetOrPost),
            Parameter.CreateParameter("rs", CreateRandomString(10), ParameterType.GetOrPost),
            Parameter.CreateParameter("user", user, ParameterType.GetOrPost),
            Parameter.CreateParameter("password", password, ParameterType.GetOrPost),
        });
        return p;
    }

    /// <summary>
    /// Gets a "snap" of the current camera video data and returns an Image
    /// </summary>
    /// <param name="timeout">Request timeout to camera in seconds</param>
    /// <returns>Image may be empty</returns>
    public Image<Rgba32> GetSnap(int timeout = 3)
    {
        var data = CreateCommand(Username, Password);
        try
        {
            var restClient = new RestClient(Url);
            var request = new RestRequest();
            request.AddOrUpdateParameters(data);
            var response = restClient.GetAsync(request).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Read bytes from responseContent
                var bytes = Encoding.ASCII.GetBytes(response.Content);
                
                // TODO DEV: Let's try to Save everything as a text-file for a moment
                var img = SixLabors.ImageSharp.Image<Rgba32>.Load(bytes);
                var savePath = @"C:\Users\Baumbart13\Desktop\ReoLink.txt";
                img.SaveAsPng(savePath);
                //var width = ((int)bytes[0] << 8) | (int)bytes[1];
                //var height = ((int)bytes[2] << 8) | (int)bytes[3];
                //File.WriteAllText(savePath, $"Width:{width}\nHeigth:{height}\n");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Could not get Image data");
            return new Image<Rgba32>(0, 0);
        }
        return new Image<Rgba32>(0, 0);
    }

    public IPAddress IpAddress { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public Profile Profile { get; set; }
    public int Port { get; set; }
    public string Url { get; set; }
}