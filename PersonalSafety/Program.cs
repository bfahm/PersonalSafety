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
        public static void Main(string[] args)
        {
            // Get current working environment
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            //Add environment variable to the array of args
            Array.Resize(ref args, args.Length + 1);
            args[args.Length - 1] = environment;

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

            bool isDevelopment = Array.FindAll(args, s => s.Equals(Environments.Development)).Length > 0;
            bool isProduction = Array.FindAll(args, s => s.Equals(Environments.Production)).Length > 0;

            if (isDevelopment)
            {
                hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://*:5566")
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
