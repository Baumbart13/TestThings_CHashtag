using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Emgu.CV;
using Emgu.CV.Structure;
using ReoLinkApiSharp.Utils;

namespace ReoLinkApiSharp.Mixins;

/// <summary>
/// API calls for opening a video stream or capturing an image from the camera.
/// </summary>
public class StreamAPIMixin
{
    private static Random random = new Random();

    public void OpenVideoStream()
    {
        var rtspClient = new RtspClient(IpAddress, Username, Password, Port, Profile);
        var stream = rtspClient.OpenStream();
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

    private static JsonObject CreateJSONCommand(string cmd, int channel, string rs, string user, string password)
    {
        var json = new JsonObject();
        json.Add("cmd", cmd);
        json.Add("channel", channel);
        json.Add("rs", rs);
        json.Add("user", user);
        json.Add("password", password);
        return json;
    }

    /// <summary>
    /// Gets a "snap" of the current camera video data and returns an Image
    /// </summary>
    /// <param name="timeout">Request timeout to camera in seconds</param>
    /// <returns>Image may be empty</returns>
    public Image<Rgba, int> GetSnap(int timeout = 3)
    {
        var data = CreateJSONCommand("Snap", 0, CreateRandomString(10), Username, Password);
        try
        {
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(timeout);
            var request = new HttpRequestMessage(HttpMethod.Get, Url);
            request.Content = new StringContent(data.ToJsonString());
            var response = httpClient.Send(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Read bytes from responseContent
                var stream = response.Content.ReadAsStream(); // TODO: Red bytes from responseContent into image
                var streamBytes = new byte[stream.Length];
                stream.Read(streamBytes);
                // TODO DEV: Let's try to Save everything as a text-file for a moment
                var sb = new StringBuilder(streamBytes.Length);
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Could not get Image data");
            return new Image<Rgba, int>(0, 0);
        }
        return new Image<Rgba, int>(0, 0);
    }

    public IPAddress IpAddress { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public Profile Profile { get; set; } = Profile.main;
    public int Port { get; set; } = 554;
    public string Url { get; set; }
}