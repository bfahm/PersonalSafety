using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs.HubTracker
{
    public static class UserHandler
    {
        public static HashSet<ConnectionInfo> ConnectionInfoSet = new HashSet<ConnectionInfo>();
    }

    public static class SOSHandler
    {
        public static HashSet<SOSInfo> SOSInfoSet = new HashSet<SOSInfo>();
    }

    public class ConnectionInfo
    {
        public string ConnectionId { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
    }

    public class SOSInfo
    {
        public string ConnectionId { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public int SOSId { get; set; }
    }
}
