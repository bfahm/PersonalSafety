﻿using PersonalSafety.Contracts.Enums;

namespace PersonalSafety.Hubs.Helpers
{
    public static class ConsoleFormatter
    {
        public static string onConnectionStateChanged(string email, string connectionId, bool hasDisconnected)
        {
            string consoleLineIntermediate = !hasDisconnected ? " has connected to the server with connection id: " 
                                                              : " has disconnected from the server, he had connection id: ";
            return email + consoleLineIntermediate + connectionId;
        }

        public static string onSOSStateChanged(string email, int sosId, StatesTypesEnum sosState, string deparment = null)
        {
            string consoleText = "";
            switch (sosState)
            {
                case StatesTypesEnum.Pending:
                    {
                        consoleText = $"Client {email} created a new request with id: {sosId}, the request was routed to {deparment}";
                        break;
                    }
                case StatesTypesEnum.Canceled:
                    {
                        consoleText = $"A client canceled his request holding SOSId: {sosId}";
                        break;
                    }
                case StatesTypesEnum.Accepted:
                    {
                        consoleText = $"An agent accepted the request with id: {sosId}";
                        break;
                    }
                case StatesTypesEnum.Solved:
                    {
                        consoleText = $"Rescuer {email} solved a request with id: {sosId}";
                        break;
                    }
            }

            return WrapSOSBusiness(consoleText);
        }

        public static string onEventStateChanged(string email, int eventId, StatesTypesEnum eventState)
        {
            string consoleText = "";
            switch (eventState)
            {
                case StatesTypesEnum.Pending:
                    {
                        consoleText = $"Client {email} created a new event with id: {eventId}.";
                        break;
                    }
                case StatesTypesEnum.Canceled:
                    {
                        consoleText = $"A client canceled his event holding SOSId: {eventId}";
                        break;
                    }
                case StatesTypesEnum.Solved:
                    {
                        consoleText = $"An event with id: {eventId} was recently marked as solved.";
                        break;
                    }
            }

            return WrapEventBusiness(consoleText);
        }

        public static string WrapSOSBusiness(string message)
        {
            message = "SOSREQUEST / " + message;
            return message;
        }

        public static string WrapEventBusiness(string message)
        {
            message = "EVENTS / " + message;
            return message;
        }

        public static string onGenericText(string email, string customText)
        {
            return email + " " + customText + ".";
        }
    }
}
