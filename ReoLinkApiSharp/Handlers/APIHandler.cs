using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ReoLinkApiSharp.Mixins;
using RestSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
    private string _token = "null";

    public Image<Rgba32> GetSnap()
    {
        return (this as IStreamAPIMixin).GetSnap();
    }

    /// <summary>
    /// If Token is null a string with value of "null" will be returned.
    /// </summary>
    public string Token
    {
        get
        {
            if (_token == "")
            {
                _token = "null";
            }

            return _token;
        }
        set => _token = value;
    }

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

    [Obsolete("Please use \"Login()\". It is the only known way to communicate correctly with the camera's API")]
    public void LoginClient()
    {
        var data = new JsonArray(new JsonObject(new[]
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

        using var client = new HttpClient();
        client.BaseAddress = new Uri(Url);
        Console.WriteLine($"Url is {client.BaseAddress}");
        var contentType = new MediaTypeWithQualityHeaderValue("application/json");
        client.DefaultRequestHeaders.Accept.Add(contentType);
        var api = $"cmd=Login&token={Token}";
        var contentData = new StringContent(data.ToJsonString(), Encoding.UTF8, contentType.ToString());

        JsonNode? response;
        using (var httpResponse = client.PostAsJsonAsync($"{Url}?{api}", contentData).Result)
            //using (var httpResponse = client.PostAsync(api, contentData).Result)
        {
            if (!httpResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Nope, there was an error");
                return;
            }

            var stringData = httpResponse.Content.ReadAsStringAsync().Result;
            response = JsonNode.Parse(stringData);
        }

        if (response == null || response.ToJsonString().Length < 1)
        {
            Console.WriteLine("Failed to login\nResponse was null");
            return;
        }

        var jsonArray = response.AsArray();
        var jsonData = jsonArray[0];
        var code = jsonData["code"].ToJsonString();
        Console.WriteLine($"code is \"{code}\"");
        if (code.Equals("0"))
        {
            Token = jsonData["value"]["Token"]["name"].ToJsonString().Replace("\"", "");
            Console.WriteLine($"Token is \"{Token}\"");
            return;
        }
    }

    /// <summary>
    /// Get login token
    /// Must be called first, before any other operation can be performed
    /// </summary>
    /// <returns>True, if login was successful</returns>
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
            var param = new Dictionary<string, string>
            {
                { "cmd", "Login" },
                { "token", Token }
            };

            var webResponse = RestHandler.Post(Url, body, param).ToHttpResponseMessage();
            var response = webResponse.ToJsonNode();

            // [
            // {
            //     "cmd" : "Login",
            //     "code" : 0,
            //     "value" : {
            //         "Token" : {
            //             "leaseTime" : 3600,
            //             "name" : "039b4d5c64f3e7a"
            //         }
            //     }
            // }
            // ]

            if (response == null || response.ToJsonString().Length < 1)
            {
                Console.WriteLine("Failed to login\nResponse was null");
                return false;
            }

            var jsonArray = response.AsArray();
            var jsonData = jsonArray[0];
            var code = jsonData["code"].ToJsonString();
            Console.WriteLine($"code is \"{code}\"");
            if (code.Equals("0"))
            {
                Token = jsonData["value"]["Token"]["name"].ToJsonString().Replace("\"", "");
                Console.WriteLine($"Token is \"{Token}\"");
                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Login\n{e}");
            throw;
        }

        return false;
    }

    public bool Logout()
    {
        try
        {
            var body = new JsonArray(new JsonObject(new[]
            {
                new KeyValuePair<string, JsonNode?>("cmd", "Logout"),
                new KeyValuePair<string, JsonNode?>("action", 0)
            }));
            ExecuteCommand("Logout", body);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Logout\n{e}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Send a POST request to the IP camera with given data
    /// </summary>
    /// <param name="cmd">Name of the command to send</param>
    /// <param name="data">Object to send to the camera (send as JSON)</param>
    /// <param name="multiStepCmd">Whether the given command name should be added to the URL parameters of the request. Defaults to false. (Some multi-step commands seem to not have a single command name)</param>
    /// <returns>Response JSON</returns>
    protected JsonNode ExecuteCommand(string cmd, JsonNode data, bool multiStepCmd = false)
    {
        var param = new Dictionary<string, string>{
            { "token", Token },
            { "cmd", cmd }
        };
        if (multiStepCmd)
        {
            param.Remove("cmd");
        }

        try
        {
            if (Token.Equals("null"))
            {
                throw new WebException("Login first");
            }

            if (!cmd.Equals("Download"))
            {
                var response = RestHandler.Post(Url, data, param).ToHttpResponseMessage();
                var jsonResponse = response.ToJsonNode() ?? new JsonObject();

                return jsonResponse;
            }

            throw new NotImplementedException("Need to implement the \"Download\" command");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Command {cmd} failed: {e}");
            throw;
        }
    }
}