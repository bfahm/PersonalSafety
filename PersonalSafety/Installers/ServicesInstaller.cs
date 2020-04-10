using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalSafety.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Services.Location;

namespace PersonalSafety.Installers
{
    public class ServicesInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register Services Here
            services.AddSingleton<IFacebookAuthService, FacebookAuthService>();
            services.AddSingleton<IGithubUpdateService, GithubUpdateService>();
            services.AddScoped<IJwtAuthService, JwtAuthService>();
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IEmailService, EmailService>();
        }
    }
}
