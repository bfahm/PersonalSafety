using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Services.PushNotification
{
    public interface IPushNotificationsService
    {
        Task<bool> TrySendNotification(string registrationToken, string title, string body);
    }
}
