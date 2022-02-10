using NLog;
using NLog.Targets;

namespace NetMQ_DesktopClient;

public class ZeroMqLogger : TargetWithLayout
{
    public struct Message
    {
        public string Content { get; set; }
        public LogLevel Level { get; set; }
    }

    public List<Message> Messages { get; set; }

    public ZeroMqLogger()
    {
        Messages = new List<Message>();
    }

    protected override void Write(LogEventInfo logEvent)
    {
        var logMsg = Layout.Render(logEvent);

        Messages.Add(new Message
        {
            Content = logMsg,
            Level = logEvent.Level
        });
    }
}