using System;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Razor.Language;
using PersonalSafety.Hubs.HubTracker;

namespace PersonalSafety.Hubs.Helpers
{
    public class HubConsoleHelper
    {
        private readonly IAdminHub _adminHub;

        public HubConsoleHelper(IAdminHub adminHub)
        {
            _adminHub = adminHub;
            TrackerHandler.ConsoleSet.CollectionChanged += ConsoleSetOnChanged;
        }

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

        private void ConsoleSetOnChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
            {
                _adminHub.PrintToOnlineConsole(item.ToString());
            }
        }
    }
}
