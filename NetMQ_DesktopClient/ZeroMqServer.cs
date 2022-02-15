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
        throw new NotImplementedException();
    }

    public void Start()
    {
        _logger.Info("Starting up server");
        _logger.Info("Loading all config-files");
        LoadAllConfig();
        _logger.Info("Preparing directories for image processing");
        CreateIODirs();
    }

    protected void CreateIODirs()
    {
        throw new NotImplementedException();
    }

    protected void LoadAllConfig()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }
}