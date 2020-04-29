using System;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Razor.Language;
using PersonalSafety.Hubs.HubTracker;

namespace PersonalSafety.Hubs.Helpers
{
    public static class HubConsoleHelper
    {
        public static string ConsoleFormater(string email, string connectionId, bool hasDisconnected)
        {
            string consoleLineIntermediate = !hasDisconnected ? " has connected to the server with connection id: " 
                                                              : " has disconnected from the server, he had connection id: ";
            return email + consoleLineIntermediate + connectionId;
        }

        public static string ConsoleFormater(string email, string customText)
        {
            return email + " " + customText + ".";
        }
    }
}
