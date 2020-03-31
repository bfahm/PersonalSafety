using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalSafety.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Installers
{
    public class BusinessesInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register Businesses
            services.AddScoped<IAccountBusiness, AccountBusiness>();
            services.AddScoped<IClientBusiness, ClientBusiness>();
            services.AddScoped<IAdminBusiness, AdminBusiness>();
            services.AddScoped<IAgentBusiness, AgentBusiness>();
            services.AddScoped<ISOSBusiness, SOSBusiness>();
            services.AddScoped<IRescuerBusiness, RescuerBusiness>();
        }
    }
}
