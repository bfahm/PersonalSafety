﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs.HubHelper
{
    public static class UserHandler
    {
        public static HashSet<ConnectionInfo> ConnectionInfoSet = new HashSet<ConnectionInfo>();
    }

    public class ConnectionInfo
    {
        public string ConnectionId { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
    }
}
