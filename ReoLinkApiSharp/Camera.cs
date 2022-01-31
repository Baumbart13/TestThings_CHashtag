using System.Net;
using System.Transactions;
using ReoLinkApiSharp.Handlers;

namespace ReoLinkApiSharp;

public class Camera : APIHandler
{
    /// <summary>
    /// Initialise the Camera object by passing the ip address.
    /// The default details {"username":"admin", "password":""} will be used if nothing passed
    /// For deferring the login to the camera, just pass defer_login = True.
    /// For connecting to the camera behind a proxy pass a proxy argument: proxy={"http": "socks5://127.0.0.1:8000"}
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="https">connect to the camera over https</param>
    /// <param name="deferLogin">defer the login process</param>
    /// <param name="proxy">Add a proxy dict for requests to consume.
    /// eg: {"http":"socks5://[username]:[password]@[host]:[port], "https": ...}
    /// More information on proxies in requests: https://stackoverflow.com/a/15661226/9313679</param>
    public Camera(IPAddress ipAddress, string username, string password, bool https = false, bool deferLogin = false,
        Profile profile = Profile.main) : base(ipAddress, username, password, https)
    {
        IpAddress = ipAddress;
        Username = username;
        Password = password;
        Profile = profile;
        
        if (!deferLogin)
        {
            base.Login();
        }
    }

    public IPAddress IpAddress { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public Profile Profile { get; set; }
}