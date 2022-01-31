using System.Net;
using System.Transactions;

namespace ReoLinkApiSharp;

public class Camera : APIHandler
{
    /// <summary>
    /// Initialise the Camera object by passing the ip address.
    /// The default details {"username":"admin", "password":""} will be used if nothing passed
    /// For deferring the login to the camera, just pass defer_login = True.
    /// For connecting to the camera behind a proxy pass a proxy argument: proxy={"http": "socks5://127.0.0.1:8000"}
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="https">connect to the camera over https</param>
    /// <param name="deferLogin">defer the login process</param>
    /// <param name="proxy">Add a proxy dict for requests to consume.
    /// eg: {"http":"socks5://[username]:[password]@[host]:[port], "https": ...}
    /// More information on proxies in requests: https://stackoverflow.com/a/15661226/9313679</param>
    public Camera(IPAddress ip, string username, string password, bool https = false, bool deferLogin = false,
        string profile = "main")
    {
        if (!deferLogin)
        {
            
        }
    }

    public IPAddress ip { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public string profile { get; set; }
}