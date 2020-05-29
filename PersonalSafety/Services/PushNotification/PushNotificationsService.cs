using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PersonalSafety.Services.PushNotification
{
    public class PushNotificationsService : IPushNotificationsService
    {
        private readonly ILogger<PushNotificationsService> _logger;
        private IWebHostEnvironment _hostingEnvironment;

        public PushNotificationsService(ILogger<PushNotificationsService> logger, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;

            try
            {
                string jsonPath = Path.Combine(_hostingEnvironment.ContentRootPath, "fcm_key.json");
                _logger.LogInformation($"FCM / Path of JSON is {jsonPath}.");

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);

                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.GetApplicationDefault(),
                });
            }
            catch
            {
                _logger.LogError("FCM / ERROR: Could not authenticate with JSON key.");
            }
        }

        public async Task<bool> TrySendNotification(string registrationToken, string title, string body)
        {
            try
            {
                // See documentation on defining a message payload.
                var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body,
                    },
                    Token = registrationToken,
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation($"FCM / SUCCESS: {response}.");

                return true;
            }
            catch
            {
                _logger.LogError("FCM / ERROR: Wrong Registration Token.");
                return false;
            }
        }
    }
}
