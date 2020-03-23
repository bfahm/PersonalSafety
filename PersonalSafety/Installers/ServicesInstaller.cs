using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalSafety.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Installers
{
    public class ServicesInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register Services Here
            services.AddSingleton<IFacebookAuthService, FacebookAuthService>();
            services.AddScoped<IJwtAuthService, JwtAuthService>();
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<IEmailService, EmailService>();
        }
    }
}
