using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

namespace SignalRChatServer.Hubs
{
    public class RealtimeHub : Hub
    {

        #region Consts, Fields, Properties, Events

        #endregion

        #region Methods

        private static Random random = new Random();
        public static string RandomString(int length)
        {

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());

        }

        public void START_LOCATION_SHARING(string connectionID, string userID ,int longitude, int latitude)
        {

            string groupName = userID + "_LOCATION_" + RandomString(10);

            //TODO : Create a group based on long and lat
            Groups.AddToGroupAsync(connectionID, groupName);

            Clients.Caller.SendCoreAsync("LOCATION_GROUP_NAME", new object[] { groupName });

            Console.WriteLine("Started sharing location of user with ID " + userID + ". \n Group name: " + groupName);

        }

        public void SHARE_LOCATION(int longitude, int latitude)
        {

            Console.WriteLine("Got location from client!");
            Console.WriteLine("Long is " + longitude);
            Console.WriteLine("Lat is " + latitude);

            //TODO : Update that user's location in the DB
            //TODO : Check if that user has an active SOS request, and based on whether he does, send his location to users in that request's group
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            Console.WriteLine("Some unknown connected to us.");
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            //await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);
            Console.WriteLine("Some unknown lost connection.");
        }

        #endregion
    }
}