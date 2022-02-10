using NetMQ.Sockets;
using NLog.Targets;

namespace NetMQ_DesktopClient;

public class ZeroMqServer
{
    public static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    protected ResponseSocket Responser;

    public ZeroMqServer()
    {
        Target.Register<ZeroMqLogger>("Server log");
    }
    
    public void Run()
    {
        
    }

    public void Start()
    {
        _logger.Info("Starting up server");
        _logger.Info("Preparing directories for image processing");
    }

    public void Stop()
    {
        
    }
}