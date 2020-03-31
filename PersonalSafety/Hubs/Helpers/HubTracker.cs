using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs.HubTracker
{
    public static class TrackerHandler
    {
        public static HashSet<string> ConsoleSet = new HashSet<string>();
        public static HashSet<ClientConnectionInfo> ClientConnectionInfoSet = new HashSet<ClientConnectionInfo>();
        public static HashSet<RescuerConnectionInfo> RescuerConnectionInfoSet = new HashSet<RescuerConnectionInfo>();
        public static HashSet<RescuerConnectionInfo> RescuerWithPendingMissionsSet = new HashSet<RescuerConnectionInfo>();
        public static HashSet<ConnectionInfo> AllConnectionInfoSet = new HashSet<ConnectionInfo>();
        
    }

    // Tracker Classes (Kind of entities to be tracked)
    
    public class ConnectionInfo
    {
        public string ConnectionId { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
    }

    public class ClientConnectionInfo : ConnectionInfo
    {
        public int SOSId { get; set; }
    }

    public class RescuerConnectionInfo : ConnectionInfo
    {
        public int DepartmentId { get; set; }
        public int CurrentJob { get; set; }
    }
}
