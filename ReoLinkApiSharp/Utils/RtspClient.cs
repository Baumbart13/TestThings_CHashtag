using System.Net;
using Emgu.CV;

namespace ReoLinkApiSharp.Utils;

/// <summary>
/// This is a wrapper of the OpenCV VideoCapture method
/// Inspiration from:
///     - https://github.com/ReolinkCameraAPI/reolinkapipy
/// </summary>
public class RtspClient
{
    /// <summary>
    /// RTSP client is used to retrieve frames from the camera in a stream
    /// </summary>
    /// <param name="ip">Camera IP</param>
    /// <param name="username">Camera Username</param>
    /// <param name="password">Camera User Password</param>
    /// <param name="port">RTSP Port</param>
    /// <param name="profile">"main" or "sub"</param>
    /// <param name="useUdp">True to use UDP, False to use TCP</param>
    public RtspClient(IPAddress ip, string username, string password, int port = 554, Profile profile = Profile.main,
        bool useUdp = true)
    {
        IpAddress = ip;
        this.Username = username;
        this.Password = password;
        this.Port = port;
        this.Url = $"rtsp://{Username}:{Password}@{IpAddress}:{Port}//h264Preview_01_{profile.ToString()}";
        var captureOption = useUdp ? "udp" : "tcp";
    }

    public RtspClient(IPAddress ip, string username, string password, List<Proxy> proxiesList, int port = 554,
        Profile profile = Profile.main,
        bool useUdp = true) : this(ip, username, password, port, profile, useUdp)
    {
        Proxies = proxiesList;
    }

    public void OpenVideoCapture()
    {
        // To CAP_FFMPEG or not to?
        Capture = new Emgu.CV.VideoCapture(Url, VideoCapture.API.Ffmpeg);
    }

    public IEnumerable<Mat> StreamBlocking()
    {
        while (true)
        {
            if (Capture.IsOpened)
            {
                var frame = new Mat();
                var ret = Capture.Read(frame);
                if (ret)
                {
                    yield return frame;
                }
            }
            else
            {
                Console.WriteLine("Stream closed");
                Capture.Stop();
                Capture.Dispose();
                break;
            }
        }
    }

    public async void StreamNonBlocking()
    {
        while (!ThreadCancelled)
        {
            try
            {
                if (Capture.IsOpened)
                {
                    var frame = new Mat();
                    var ret = Capture.Read(frame);
                    if (ret)
                    {
                        // Callback with frame
                    }
                }
                else
                {
                    Console.WriteLine("Stream is closed");
                    StopStream();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                StopStream();
            }
        }
    }

    public void StopStream()
    {
        Capture.Stop();
        Capture.Dispose();
        ThreadCancelled = true;
    }

    /// <summary>
    /// Opens OpenCV video stream and returns the result according to theOpenCV documentation
    /// </summary>
    public IEnumerable<Mat> OpenStream()
    {
        // Reset the capture object
        if (Capture == null || !Capture.IsOpened)
        {
            OpenVideoCapture();
        }
        
        Console.WriteLine("Opening stream");
        
        // Only if there is no callback, return blocking
        // otherwise return blocking, if it is on a new task
        // if it's even not that, return non-blocking stream
        //
        // Since there are no callbacks implemented, we can
        // jump right onto returning blocking stream
        return StreamBlocking();
    }

    public IPAddress IpAddress { get; set; }
    public VideoCapture Capture { get; set; }
    public bool ThreadCancelled { get; set; } = false;
    public string Username { get; set; }
    public string Password { get; set; }
    public int Port { get; set; }
    public List<Proxy> Proxies { get; set; }
    public string Url { get; set; }
}