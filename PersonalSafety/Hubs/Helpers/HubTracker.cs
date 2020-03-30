using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs.HubTracker
{
    public static class TrackerHandler
    {
        public static HashSet<ConnectionInfo> ConnectionInfoSet = new HashSet<ConnectionInfo>();
        public static HashSet<SOSInfo> SOSInfoSet = new HashSet<SOSInfo>();
        public static HashSet<string> ConsoleSet = new HashSet<string>();
        public static HashSet<RescuerConnectionInfo> RescuerConnectionInfoSet = new HashSet<RescuerConnectionInfo>();
        public static HashSet<RescuerConnectionInfo> RescuerWithPendingMissionsSet = new HashSet<RescuerConnectionInfo>();
    }

    // Tracker Classes (Kind of entities to be tracked)
    
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

    public class RescuerConnectionInfo
    {
        public string ConnectionId { get; set; }
        public string UserEmail { get; set; }
        public int DepartmentId { get; set; }
        public int CurrentJob { get; set; }
    }
}
