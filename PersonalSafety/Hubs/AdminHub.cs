using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Hubs.HubTracker;
using System.Collections.Specialized;
using PersonalSafety.Services.PushNotification;

namespace PersonalSafety.Hubs
{
    [Authorize(Roles = Roles.ROLE_ADMIN)]
    public class AdminHub : MainHub
    {
        private readonly IPushNotificationsService _pushNotificationsService;
        private const string AdminConsoleChannel = "AdminConsoleChanges";
        private const string AdminFCMChannel = "AdminFCMChannel";

        public AdminHub(IPushNotificationsService pushNotificationsService)
        {
            TrackerHandler.ConsoleSet.CollectionChanged += ConsoleSetOnChanged;
            _pushNotificationsService = pushNotificationsService;
        }

        public async Task ToggleFCMMasterSwitch()
        {
            var newValue = _pushNotificationsService.ToggleMasterSwitch();
            await Clients.All.SendAsync(AdminFCMChannel, newValue);
        }

        public async Task GetFCMMasterSwitchValue()
        {
            var currentValue = _pushNotificationsService.GetMasterSwitch();
            await Clients.All.SendAsync(AdminFCMChannel, currentValue);
        }

        public async Task SendTestNotification(string registrationToken, string title, string body)
        {
            await _pushNotificationsService.SendNotification(registrationToken, title, body);
        }

        private void PrintToOnlineConsole(string text)
        {
            Clients.All.SendAsync(AdminConsoleChannel, text);
        }


        private void ConsoleSetOnChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
            {
                PrintToOnlineConsole(item.ToString());
            }
        }

        public override async Task OnConnectedAsync()
        {
            await GetFCMMasterSwitchValue();
            await base.OnConnectedAsync();
        }
    }
}
