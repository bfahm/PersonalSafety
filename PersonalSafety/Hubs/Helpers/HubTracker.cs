using System.Collections.Generic;

namespace PersonalSafety.Hubs.HubTracker
{
    public static class TrackerHandler
    {
        public static HashSet<string> ConsoleSet;
        public static HashSet<ClientConnectionInfo> ClientConnectionInfoSet;
        public static HashSet<AgentConnectionInfo> AgentConnectionInfoSet;
        public static HashSet<RescuerConnectionInfo> RescuerConnectionInfoSet;
        public static HashSet<RescuerConnectionInfo> RescuerWithPendingMissionsSet;
        public static HashSet<ConnectionInfo> AllConnectionInfoSet;

        static TrackerHandler()
        {
            InitializeConsoleLog();
            InitializeTrackers();
        }

        public static void InitializeConsoleLog()
        {
            ConsoleSet = new HashSet<string>();
        }

        public static void InitializeTrackers()
        {
            ClientConnectionInfoSet = new HashSet<ClientConnectionInfo>();
            AgentConnectionInfoSet = new HashSet<AgentConnectionInfo>();
            RescuerConnectionInfoSet = new HashSet<RescuerConnectionInfo>();
            RescuerWithPendingMissionsSet = new HashSet<RescuerConnectionInfo>();
            AllConnectionInfoSet = new HashSet<ConnectionInfo>();
        }
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

    public class AgentConnectionInfo : ConnectionInfo
    {
        public int DepartmentId { get; set; }
    }
}
