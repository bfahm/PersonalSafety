using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Hubs.HubTracker;

namespace PersonalSafety.Hubs.Helpers
{
    public static class HubTools
    {
        public static void PrintToConsole(string email, string connectionId, bool hasDisconnected)
        {
            string consoleLineIntermediate = !hasDisconnected ? " has connected to the server with connection id: " 
                                                              : " has disconnected from the server, he had connection id: ";
            string consoleLine = email + consoleLineIntermediate + connectionId;
            PrintFactorizedString(consoleLine);
        }

        public static void PrintToConsole(string email, string customText)
        {
            string consoleLine = email + " " + customText + ".";
            PrintFactorizedString(consoleLine);
        }

        private static void PrintFactorizedString(string factorizedText)
        {
            factorizedText = DateTime.UtcNow.ToLongTimeString() + " " +  DateTime.UtcNow.ToShortDateString() + " | " + factorizedText;
            TrackerHandler.ConsoleSet.Add(factorizedText);
            Console.WriteLine(factorizedText);
        }
    }

    
}
