using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Hubs.HubTracker;
using System.Collections.Specialized;

namespace PersonalSafety.Hubs
{
    [Authorize(Roles = Roles.ROLE_ADMIN)]
    public class AdminHub : MainHub
    {
        public AdminHub()
        {
            TrackerHandler.ConsoleSet.CollectionChanged += ConsoleSetOnChanged;
        }

        private void PrintToOnlineConsole(string text)
        {
            Clients.All.SendAsync("AdminConsoleChanges", text);
        }


        private void ConsoleSetOnChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
            {
                PrintToOnlineConsole(item.ToString());
            }
        }
    }
}
