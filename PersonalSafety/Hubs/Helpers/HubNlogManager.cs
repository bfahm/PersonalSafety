using NLog;
using NLog.Config;
using NLog.Targets;
using PersonalSafety.Hubs.HubTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs.Helpers
{
    [Target("HubNlogManager")]
    public class HubNlogManager : TargetWithLayout
    {
        [RequiredParameter]
        public string Host { get; set; }

        public HubNlogManager()
        {
            this.Host = "localhost";
        }

        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = Layout.Render(logEvent);
            SendTheMessageToRemoteHost(logMessage);
        }

        private void SendTheMessageToRemoteHost(string message)
        {
            TrackerHandler.ConsoleSet.Add(message);
        }
    }
}
