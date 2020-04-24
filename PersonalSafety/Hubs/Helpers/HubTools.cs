﻿using System;
using PersonalSafety.Hubs.HubTracker;

namespace PersonalSafety.Hubs.Helpers
{
    public class HubTools : IHubTools
    {
        private readonly IAdminHub _adminHub;

        public HubTools(IAdminHub adminHub)
        {
            _adminHub = adminHub;
        }

        public void PrintToConsole(string email, string connectionId, bool hasDisconnected)
        {
            string consoleLineIntermediate = !hasDisconnected ? " has connected to the server with connection id: " 
                                                              : " has disconnected from the server, he had connection id: ";
            string consoleLine = email + consoleLineIntermediate + connectionId;
            PrintFactorizedString(consoleLine);
        }

        public void PrintToConsole(string email, string customText)
        {
            string consoleLine = email + " " + customText + ".";
            PrintFactorizedString(consoleLine);
        }

        public void PrintToConsole(string customText)
        {
            PrintFactorizedString(customText);
        }

        private void PrintFactorizedString(string factorizedText)
        {
            factorizedText = DateTime.UtcNow.AddHours(2).ToLongTimeString() + " " +  DateTime.UtcNow.ToShortDateString() + " | " + factorizedText;
            TrackerHandler.ConsoleSet.Add(factorizedText);
            Console.WriteLine(factorizedText);
            _adminHub.PrintToOnlineConsole(factorizedText);
        }
    }

    
}
