using System.Net;
using NetMQ;
using NetMQ.Sockets;

namespace Network;

public class ServerSocket : IDisposable
{
    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    private NetMQPoller poller;
    private RouterSocket server;
    private Thread thread;
    private bool alive = false;

    public ServerSocket()
    {
        poller = new NetMQPoller();
        server = new RouterSocket();
    }
    
    public void Start()
    {
        alive = true;
        thread = new Thread(Running);
        thread.Start();
    }

    private void Running()
    {
        var address = IPEndPoint.Parse(String.Empty); //TODO: Update ServerSocket
    }

    public void Stop()
    {
        alive = false;
        thread.Interrupt();
        server.Close();
        thread.Join();
        thread = null;
    }

    #region IDisposable
    private bool isDisposed = false;
    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                server.Dispose();
                poller.Dispose();
            }

            isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
    #endregion
}