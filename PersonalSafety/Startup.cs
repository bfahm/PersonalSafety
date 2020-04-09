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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IClientRepository clientRepository, IPersonnelRepository personnelRepository, IDepartmentRepository departmentRepository)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (env.IsProduction())
            {
                // APIResponses that support HTTP 500
                app.ConfigureExceptionHandler();
                
                // APIResponses that support HTTP 401 and HTTP 404
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            app.UseRouting();

            //Allow headers required by SignalR, order is important
            app.Use((context, next) =>
            {
                context.Response.Headers["Access-Control-Allow-Origin"] = context.Request.Headers.Where(h => h.Key == "Origin").FirstOrDefault().Value.ToString();
                context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                context.Response.Headers["Access-Control-Allow-Methods"] = "*";
                context.Response.Headers["Access-Control-Allow-Headers"] = "Authorization, X-Requested-With, Content-Type";

                return next.Invoke();
            });

            app.UseCors();

            app.UseStaticFiles();

            serviceProvider.GetService<AppDbContext>().Database.EnsureCreated();
            
            ApplicationDbInitializer databaseInitializer = new ApplicationDbInitializer(userManager, roleManager, clientRepository, personnelRepository, departmentRepository);
            databaseInitializer.SeedUsers();

            app.UseSwagger();
            app.UseSwaggerUI(option => option.SwaggerEndpoint("/swagger/v1/swagger.json", "PersonalSafetyAPI Documentations"));

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                HubsInstaller.MapToEndpoints(endpoints);
            });

            //app.UseMvc();
        }
    }
}
