using System.Collections;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ReoLinkApiSharp.Handlers;

public static class RestHandler
{

    private static string ProcessUrlParams(Dictionary<string, string> urlParameters)
    {
        var param = "";
        foreach (var (key, value) in urlParameters)
        {
            param = $"{param}&{key}={value}";
        }

        return param.Remove(0, 1);
    }
    
    public static HttpResponseMessage Post(string url, JsonNode data, Dictionary<string, string> urlParameters = null!)
    {
        try
        {
            var param = ProcessUrlParams(urlParameters);
            var httpWebRequest = WebRequest.CreateHttp($"{url}?{param}");
            Console.WriteLine($"RestHandler: request-url is {httpWebRequest.RequestUri}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = data.ToJsonString();
                streamWriter.Write(json);
            }

            JsonNode? response;
            using (var webResponse = httpWebRequest.GetResponse())
            using (var streamReader = new StreamReader(webResponse.GetResponseStream()))
            {
                Console.WriteLine("WebResponse Headers");
                foreach (var x in webResponse.Headers)
                {
                    Console.WriteLine(x);
                }
                Console.WriteLine("End of Headers\n===========");
                var responseString = streamReader.ReadToEnd();
                response = JsonNode.Parse(responseString);
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Post error\n{e}");
        }

        return new HttpResponseMessage();
    }
    
    /*public static HttpResponseMessage Get(IPAddress Url, DictionaryBase jsonData,
        Dictionary<string, string> parameters = null)
    {
        try
        {
            var param = ProcessParameters(parameters);
            httpWebRequest = new HttpRequestMessage(HttpMethod.Get, $"{Url}{param}");
            Console.WriteLine($"RestHandler: request-url is {httpWebRequest.RequestUri}");
            
            var header = ("content-type", "application/json");
            httpWebRequest.Headers.Add(header.Item1, header.Item2);
            
            Console.WriteLine("Writing httpRequestOptions");
            foreach (var httpRequestOption in httpWebRequest.Options)
            {
                Console.WriteLine($"{httpRequestOption.Key}:{httpRequestOption.Value}");
            }

            var json = JsonSerializer.Serialize(jsonData);
            var contentData = new StringContent(json, Encoding.UTF8, header.Item2);
            httpWebRequest.Content = contentData;

            var response = client.Send(httpWebRequest);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            throw new ArgumentException($"Http Request had non-200 Status: {response.StatusCode}");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Post error\n{e}");
        }

        return new HttpResponseMessage();
    }*/
}