using System.Collections;
using System.Data;
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
        if (urlParameters == null)
        {
            return "";
        }
        var param = "";
        foreach (var (key, value) in urlParameters)
        {
            param = $"{param}&{key}={value}";
        }

        return param.Remove(0, 1);
    }

    internal static HttpResponseMessage ToHttpResponseMessage(this HttpWebResponse webResponse)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        using (var reader = new StreamReader(webResponse.GetResponseStream()))
        {
            var objText = reader.ReadToEnd();
            response.Content = new StringContent(objText, Encoding.UTF8, "application/json");
        }

        return response;
    }
    
    public static HttpWebResponse Post(string url, JsonNode data, Dictionary<string, string> urlParameters = null!)
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
            var webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            if (webResponse.StatusCode == HttpStatusCode.OK)
            {
                // Could be converted to HttpResponseMessage straight away, but
                // for images, the HttpWebResponse is needed
                // Otherwise it is only possible to start reading somewhere in
                // the middle of the image that was sent
                return webResponse;
            }

            throw new WebException($"Http Request had non-200 Status: {(int)webResponse.StatusCode}");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Post error\n{e}");
            throw;
        }
    }

    public static JsonNode? ToJsonNode(this HttpResponseMessage httpMsg)
    {
        JsonNode? response;
        var contentString = httpMsg.Content.ReadAsStringAsync().Result;
        response = JsonNode.Parse(contentString);
        return response;
    }
    
    public static HttpWebResponse Get(string url, Dictionary<string, string> urlParameters = null!)
    {
        try
        {
            var param = ProcessUrlParams(urlParameters);
            var httpWebRequest = WebRequest.CreateHttp($"{url}?{param}");
            Console.WriteLine($"RestHandler: request-url is {httpWebRequest.RequestUri}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            /*using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = data.ToJsonString();
                streamWriter.Write(json);
            }*/

            var webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            if (webResponse.StatusCode == HttpStatusCode.OK)
            {
                // Could be converted to HttpResponseMessage straight away, but
                // for images, the HttpWebResponse is needed
                // Otherwise it is only possible to start reading somewhere in
                // the middle of the image that was sent
                return webResponse;
            }

            throw new WebException($"Http Request had non-200 Status: {(int)webResponse.StatusCode}");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Get error\n{e}");
            throw;
        }
    }
}