﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalSafety.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Extensions;

namespace PersonalSafety.Installers
{
    public class FunctionalityInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Required for API functionality
            services.AddControllers();

            // Registering Extensions
            //services.AddScoped<GithubUpdateCheckerFilter>();

            // Needed to display the home page "view"
            services.AddMvc().AddMvcOptions(options => {
                //options.Filters.AddService(typeof(GithubUpdateCheckerFilter));
            }).AddSessionStateTempDataProvider();
            services.AddSession();

            // Registering APP Settings
            var appSettings = new AppSettings();
            configuration.Bind(nameof(AppSettings), appSettings);
            services.AddSingleton(appSettings);

            // Add Services required by FacebookAuth
            services.AddHttpClient();

            // Registering FacebookAuth Settings
            var facebookAuthSettings = new FacebookAuthSettings();
            configuration.Bind(nameof(FacebookAuthSettings), facebookAuthSettings);
            services.AddSingleton(facebookAuthSettings);

            // Register the DB Initializer
            services.AddScoped<IApplicationDbInitializer, ApplicationDbInitializer>();
        }
    }
}
