using System.Collections.Generic;
using NLog;
using NLog.Targets;

namespace ModifyColors
{
    public class ModLogger : TargetWithLayout
    {
        public struct Message
        {
            public string Content { get; set; }
            public LogLevel Level { get; set; }
        }
        
        public List<Message> Messages { get; set; }

        public ModLogger()
        {
            Messages = new List<Message>();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var logMessage = Layout.Render(logEvent);
            
            Messages.Add(new Message
            {
                Content = logMessage,
                Level = logEvent.Level,
            });
        }
    }
}