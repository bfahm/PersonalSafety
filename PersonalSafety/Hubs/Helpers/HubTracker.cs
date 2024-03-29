﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;

namespace PersonalSafety.Hubs.HubTracker
{
    public static class TrackerHandler
    {
        public static ObservableHashSet<string> ConsoleSet = new ObservableHashSet<string>();
        public static HashSet<ClientConnectionInfo> ClientConnectionInfoSet;
        public static HashSet<AgentConnectionInfo> AgentConnectionInfoSet;
        public static HashSet<RescuerConnectionInfo> RescuerConnectionInfoSet;
        public static HashSet<RescuerConnectionInfo> RescuerWithPendingMissionsSet;
        public static HashSet<ConnectionInfo> AllConnectionInfoSet;

        public static HashSet<ActiveGroup> ActiveGroups;

        static TrackerHandler()
        {
            InitializeConsoleLog();
            InitializeTrackers();
        }

        public static void InitializeConsoleLog()
        {
            ConsoleSet.Clear();
        }

        public static void InitializeTrackers()
        {
            ClientConnectionInfoSet = new HashSet<ClientConnectionInfo>();
            AgentConnectionInfoSet = new HashSet<AgentConnectionInfo>();
            RescuerConnectionInfoSet = new HashSet<RescuerConnectionInfo>();
            RescuerWithPendingMissionsSet = new HashSet<RescuerConnectionInfo>();
            AllConnectionInfoSet = new HashSet<ConnectionInfo>();

            ActiveGroups = new HashSet<ActiveGroup>();
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
        public string DepartmentName { get; set; }
        public int CurrentJob { get; set; }
    }

    public class AgentConnectionInfo : ConnectionInfo
    {
        public string DepartmentName { get; set; }
    }

    public class ActiveGroup
    {
        public string DepartmentName { get; set; }
        public int ActiveRescuers { get; set; }
    }
}
