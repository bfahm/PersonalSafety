using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Services.PushNotification
{
    public class PushNotificationsService : IPushNotificationsService
    {
        private readonly ILogger<PushNotificationsService> _logger;
        private IWebHostEnvironment _hostingEnvironment;
        private static bool MasterSwitch = false;
        private Dictionary<string, string> InjectedDict = new Dictionary<string, string>
        {
            {"click_action", "FLUTTER_NOTIFICATION_CLICK"},
            {"title", "TITLE"},
            {"body", "BODY"}
        };

        public PushNotificationsService(ILogger<PushNotificationsService> logger, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;

            try
            {
                string jsonPath = Path.Combine(_hostingEnvironment.ContentRootPath, "fcm_key.json");

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);

                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.GetApplicationDefault(),
                });

                _logger.LogInformation($"FCM / Authentication Success!");
            }
            catch
            {
                _logger.LogError("FCM / ERROR: Could not authenticate with JSON key.");
            }
        }

        public async Task SendNotification(string registrationToken, string title, string body)
        {
            var message = new Message()
            {
                Notification = new Notification()
                {
                    Title = title,
                    Body = body,
                },
                Token = registrationToken,
            };

            await TryPushNotification(message);
        }

        public async Task SendNotification(string registrationToken, Dictionary<string, string> data)
        {
            AddRangeToDictionary(ref data, ref InjectedDict);

            var message = new Message()
            {
                Data = data,
                Token = registrationToken,
            };

            await TryPushNotification(message);
        }

        public async Task SendNotification(string registrationToken, string title, string body, Dictionary<string, string> data)
        {
            AddRangeToDictionary(ref data, ref InjectedDict);

            var message = new Message()
            {
                Notification = new Notification()
                {
                    Title = title,
                    Body = body,
                },
                Data = data,
                Token = registrationToken,
            };

            await TryPushNotification(message);
        }

        private void AddRangeToDictionary(ref Dictionary<string, string> sourceDict, ref Dictionary<string, string> toBeAdded)
        {
            foreach (var item in toBeAdded)
            {
                sourceDict.Add(item.Key, item.Value);
            }
        }

        private async Task TryPushNotification(Message message)
        {
            if (MasterSwitch)
            {
                try
                {
                    string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                    _logger.LogInformation($"FCM / SUCCESS: {response}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError("FCM / " + ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("FCM / MasterSwitch is turned off, activate the service first.");
            }
        }

        public bool ToggleMasterSwitch()
        {
            MasterSwitch = !MasterSwitch;

            _logger.LogInformation($"FCM / MasterSwitch: {MasterSwitch}.");

            return MasterSwitch;
        }

        public bool GetMasterSwitch()
        {
            return MasterSwitch;
        }
    }
}
