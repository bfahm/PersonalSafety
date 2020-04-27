using NLog;
using NLog.Config;
using NLog.Targets;
using PersonalSafety.Hubs.HubTracker;

namespace PersonalSafety.Hubs.Helpers
{
    [Target("HubNlogHelper")]
    public sealed class HubNlogHelper : TargetWithLayout
    {
        [RequiredParameter]
        public string Host { get; set; }

        public HubNlogHelper()
        {
            Host = "localhost";
        }

        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = Layout.Render(logEvent);
            TrackerHandler.ConsoleSet.Add(logMessage);
        }
    }
}
