using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PersonalSafety.Models;
using PersonalSafety.Installers;
using PersonalSafety.Extensions;
using Microsoft.Extensions.Logging;
using PersonalSafety.Hubs;
using PersonalSafety.Hubs.Helpers;
using PersonalSafety.Options;

namespace PersonalSafety
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Define a list of installers, where they are:
            // - Accessible from the same assembly of that of Startup.cs
            // - A type of IInstaller Interface
            // - Not Empty interfaces
            // - Not abstract classes
            // And then create instances of each of them and add them to a list.
            var installers = typeof(Startup).Assembly.ExportedTypes.Where(i =>
                typeof(IInstaller).IsAssignableFrom(i) && !i.IsInterface && !i.IsAbstract)
                .Select(Activator.CreateInstance).Cast<IInstaller>().ToList();

            // Run the function InstallServices in each object in the list.
            installers.ForEach(installer => installer.InstallServices(services, Configuration));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, ILogger<Startup> logger, AppSettings appSettings, IApplicationDbInitializer applicationDbInitializer)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            if (env.IsProduction())
            {
                // APIResponses that support HTTP 500
                app.ConfigureExceptionHandler(logger);
                // APIResponses that support HTTP 401 and HTTP 404
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            app.UseRouting();
            app.UseCustomHeaderInjector();
            app.UseCors();

            serviceProvider.GetService<AppDbContext>().Database.EnsureCreated();
            applicationDbInitializer.SeedData();

            app.UseSwagger();
            app.UseSwaggerUI(option => option.SwaggerEndpoint("/swagger/v1/swagger.json", "PersonalSafetyAPI Documentations"));

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCustomStaticFile(appSettings);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                HubsInstaller.MapToEndpoints(endpoints);
            });
        }
    }
}
