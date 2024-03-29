using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;

namespace PersonalSafety
{
    public class Program
    {
        private static string environment = "Production";
        public static void Main(string[] args)
        {
            // Get current working environment
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
                        .ConfigureLogging((hostingContext, logging) =>
                        {
                            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                            logging.AddConsole();
                            logging.AddDebug();
                            logging.AddEventSourceLogger();
                            // Enable NLog as one of the Logging Provider
                            logging.AddNLog();
                        });

            bool isDevelopment = environment == Environments.Development;
            bool isProduction = environment == Environments.Production;
            
            if (isDevelopment)
            {
                hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://*:5000")
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>();
                });
            }
            else
            {
                hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
            }

            return hostBuilder;
        }
    }
}
