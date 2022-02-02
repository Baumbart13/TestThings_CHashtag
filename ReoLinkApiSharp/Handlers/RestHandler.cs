using System.Collections;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ReoLinkApiSharp.Handlers;

public static class RestHandler
{
    private static HttpClient client = new HttpClient();
    private static HttpRequestMessage request = new HttpRequestMessage();

    private static string ProcessParameters(Dictionary<string, string> parameters)
    {
        var paramString = "";
        if (parameters == null)
        {
            return paramString;
        }

        foreach (var parameter in parameters)
        {
            paramString = $"&{parameter.Key}={parameter.Value}{paramString}";
        }

        if (paramString.Length != 0)
        {
            var index = paramString.IndexOf("&");
            paramString = paramString.Remove(index, 1);
            paramString = $"?{paramString}";
        }

        return paramString;
    }
    
    public static HttpResponseMessage Post(string url, string jsonData,
        Dictionary<string, string> parameters = null)
    {
        try
        {
            var param = ProcessParameters(parameters);
            request = new HttpRequestMessage(HttpMethod.Post, $"{url}{param}");
            Console.WriteLine($"RestHandler: request-url is {request.RequestUri}");
            
            var header = ("content-type", "application/json");
            request.Headers.Add(header.Item1, header.Item2);
            
            Console.WriteLine("Writing httpRequestOptions");
            foreach (var httpRequestOption in request.Options)
            {
                Console.WriteLine($"{httpRequestOption.Key}:{httpRequestOption.Value}");
            }

            var contentData = new StringContent(jsonData, Encoding.UTF8, header.Item2);
            request.Content = contentData;

            var response = client.Send(request);
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
    }
    
    public static HttpResponseMessage Get(IPAddress Url, DictionaryBase jsonData,
        Dictionary<string, string> parameters = null)
    {
        try
        {
            var param = ProcessParameters(parameters);
            request = new HttpRequestMessage(HttpMethod.Get, $"{Url}{param}");
            Console.WriteLine($"RestHandler: request-url is {request.RequestUri}");
            
            var header = ("content-type", "application/json");
            request.Headers.Add(header.Item1, header.Item2);
            
            Console.WriteLine("Writing httpRequestOptions");
            foreach (var httpRequestOption in request.Options)
            {
                Console.WriteLine($"{httpRequestOption.Key}:{httpRequestOption.Value}");
            }

            var json = JsonSerializer.Serialize(jsonData);
            var contentData = new StringContent(json, Encoding.UTF8, header.Item2);
            request.Content = contentData;

            var response = client.Send(request);
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
    }
}