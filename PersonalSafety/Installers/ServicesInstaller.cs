using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalSafety.Services;
using PersonalSafety.Services.Location;
using PersonalSafety.Services.FileManager;
using PersonalSafety.Services.PushNotification;

namespace PersonalSafety.Installers
{
    public class ServicesInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register Services Here
            services.AddSingleton<IFacebookAuthService, FacebookAuthService>();
            services.AddSingleton<IGithubUpdateService, GithubUpdateService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFileManagerService, FileManagerService>();
            services.AddSingleton<IPushNotificationsService, PushNotificationsService>();
        }
    }
}
