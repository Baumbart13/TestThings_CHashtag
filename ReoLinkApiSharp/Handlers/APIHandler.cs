using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using ReoLinkApiSharp.Mixins;
using RestSharp;

namespace ReoLinkApiSharp.Handlers;

/// <summary>
/// The APIHandler class is the backend part of the API, the actual API calls
/// are implemented in Mixins.
/// This handles communication directly with the camera.
/// </summary>
public class APIHandler : IDeviceAPIMixin, IDisplayAPIMixin, IDownloadAPIMixin, IImageAPIMixin, INetworkAPIMixin,
    IRecordAPIMixin, ISystemAPIMixin, IUserAPIMixin, IZoomAPIMixin, IStreamAPIMixin
{
    public IPAddress IpAddress { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public Profile Profile { get; set; }
    public int Port { get; set; }
    public string Url { get; set; }
    public string Token { get; set; }

    /// <summary>
    /// Initialize the Camera API Handler (maps api calls into pthon)
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="https">Conenct over https</param>
    public APIHandler(IPAddress ipAddress, string username, string password, bool https = false)
    {
        var protocol = https ? "https" : "http";
        Url = $"{protocol}://{ipAddress}/cgi-bin/api.cgi";
        IpAddress = ipAddress;
        Username = username;
        Password = password;
    }

    private ParametersCollection CreateBody(string username, string password)
    {
        var p = new ParametersCollection(new[]
        {
            Parameter.CreateParameter("cmd", "Login", ParameterType.RequestBody),
            Parameter.CreateParameter("action", 0, ParameterType.RequestBody), 
            Parameter.CreateParameter("param",
                Parameter.CreateParameter("User", new ParametersCollection(new[]
                {
                    Parameter.CreateParameter("userName", username, ParameterType.RequestBody),
                    Parameter.CreateParameter("password", password, ParameterType.RequestBody), 
                }), ParameterType.RequestBody),
                ParameterType.RequestBody)
        });
        return p;
    }

    /// <summary>
    /// Return login token
    /// Must be called first, before any other operation can be performed
    /// </summary>
    /// <returns></returns>
    public bool Login()
    {
        Console.WriteLine("Logging in");
        try
        {
            //var body = CreateBody(Username, Password);
            var body = new JsonArray(new JsonObject(new[]
            {
                new KeyValuePair<string, JsonNode?>("cmd", "Login"),
                new KeyValuePair<string, JsonNode?>("action", 0),
                new KeyValuePair<string, JsonNode?>("param", new JsonObject(new[]
                {
                    new KeyValuePair<string, JsonNode?>("User", new JsonObject(new[]
                    {
                        new KeyValuePair<string, JsonNode?>("userName", "admin"),
                        new KeyValuePair<string, JsonNode?>("password", "Orangensaft")
                    }))
                }))
            }));

            var client = new RestClient($"{Url}?cmd=Login");
            Console.WriteLine($"URL is {client.BuildUri(new RestRequest())}");
            var request = new RestRequest();
            Console.WriteLine($"JSON is\n{body.ToJsonString()}");
            request.AddParameter("application/json", body.ToJsonString(), ParameterType.RequestBody);
            File.WriteAllText(@"C:\Users\Baumbart13\Desktop\Request.txt", JsonSerializer.Serialize(request));
            var response = client.PostAsync(request).Result;
            Console.WriteLine("Writing response to file");
            File.WriteAllText(@"C:\Users\Baumbart13\Desktop\ResponseContent.txt", response.Content);
            return !response.Content.Contains("error");

            if (response.IsSuccessful)
            {
                var data = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(response.Content)[0];
                var code = Convert.ToInt32(data["code"]);
                if (code == 0)
                {

                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Login\n{e}");
            return false;
        }

        return false;
    }
}