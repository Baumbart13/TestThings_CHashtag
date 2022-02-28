using System.Net;
using System.Text;
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
            
            var httpWebRequest = HttpWebRequest.CreateHttp($"{Url}?cmd=Login&token=null");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = body.ToJsonString();
                streamWriter.Write(json);
            }

            JsonNode? response;
            using (var streamReader = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()))
            {
                var responseString = streamReader.ReadToEnd();
                response = JsonNode.Parse(responseString);
            }

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
}