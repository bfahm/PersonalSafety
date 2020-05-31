using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Services.PushNotification
{
    public interface IPushNotificationsService
    {
        Task SendNotification(string registrationToken, string title, string body);
        Task SendNotification(string registrationToken, Dictionary<string, string> data);
        bool ToggleMasterSwitch();
        bool GetMasterSwitch();
    }
}
