using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Hubs.HubTracker;
using System.Collections.Specialized;
using PersonalSafety.Services.PushNotification;
using PersonalSafety.Models;
using Microsoft.Extensions.Logging;

namespace PersonalSafety.Hubs
{
    [Authorize(Roles = Roles.ROLE_ADMIN)]
    public class AdminHub : MainHub
    {
        private readonly IPushNotificationsService _pushNotificationsService;
        private readonly ILogger<AdminHub> _logger;

        private const string AdminConsoleChannel = "AdminConsoleChanges";
        private const string AdminFCMChannel = "AdminFCMChannel";
        private const string AdminClientTrackingChannel = "AdminClientTrackingChannel";

        public AdminHub(IPushNotificationsService pushNotificationsService, ILogger<AdminHub> logger)
        {
            TrackerHandler.ConsoleSet.CollectionChanged += ConsoleSetOnChanged;
            _pushNotificationsService = pushNotificationsService;
            _logger = logger;
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


        public async Task SetMinutesSkew(int newValue)
        {
            ClientTrackingRepository.MinutesSkew = newValue;
            await Clients.All.SendAsync(AdminClientTrackingChannel, newValue);
            _logger.LogInformation($"Minutes Skew set to {newValue}");
        }

        public async Task GetMinutesSkew()
        {
            var currentValue = ClientTrackingRepository.MinutesSkew;
            await Clients.All.SendAsync(AdminClientTrackingChannel, "minutes", currentValue);
        }

        public async Task SetMetersSkew(int newValue)
        {
            ClientTrackingRepository.MetersSkew = newValue;
            await Clients.All.SendAsync(AdminClientTrackingChannel, newValue);
            _logger.LogInformation($"Meters Skew set to {newValue}");
        }

        public async Task GetMetersSkew()
        {
            var currentValue = ClientTrackingRepository.MetersSkew;
            await Clients.All.SendAsync(AdminClientTrackingChannel, "meters", currentValue);
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
            await GetMinutesSkew();
            await GetMetersSkew();

            await base.OnConnectedAsync();
        }
    }
}
