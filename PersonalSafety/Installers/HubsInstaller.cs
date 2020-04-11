using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalSafety.Hubs;
using SignalRChatServer.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Hubs.Services;

namespace PersonalSafety.Installers
{
    /// <remarks>
    /// IMPORTANT:
    /// To register a new hub:
    /// - Add hub CLASS name to a new KEY-VALUE pair as the Key
    /// - Add the needed url as the Value
    /// - Register the key-value pair in a new endpoint in MapToEndpoints()
    /// - Register the hub as a Scoped Service in InstallService()
    /// - Note that the urls dictionary is used elsewhere not just in the hubs installer.
    /// </remarks>
    public class HubsInstaller : IInstaller
    {
        public static Dictionary<string, string> urls = new Dictionary<string, string>
        {
            {nameof(ClientHub), "/hubs/Client"},
            {nameof(AdminHub), "/hubs/Admin"},
            {nameof(AgentHub), "/hubs/Agent"},
            {nameof(RescuerHub), "/hubs/Rescuer"},
            {nameof(RealtimeHub), "/hubs/Realtime"}
        };

        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
                hubOptions.KeepAliveInterval = TimeSpan.MinValue;
            });

            // Register Hubs Here
            services.AddScoped<IMainHub, MainHub>();
            services.AddScoped<IClientHub, ClientHub>();
            services.AddScoped<IAgentHub, AgentHub>();
            services.AddScoped<IRescuerHub, RescuerHub>();
        }

        public static void MapToEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHub<RealtimeHub>(urls.FirstOrDefault(u => u.Key == nameof(RealtimeHub)).Value);
            endpoints.MapHub<ClientHub>(urls.FirstOrDefault(u => u.Key == nameof(ClientHub)).Value);
            endpoints.MapHub<AdminHub>(urls.FirstOrDefault(u => u.Key == nameof(AdminHub)).Value);
            endpoints.MapHub<AgentHub>(urls.FirstOrDefault(u => u.Key == nameof(AgentHub)).Value);
            endpoints.MapHub<RescuerHub>(urls.FirstOrDefault(u => u.Key == nameof(RescuerHub)).Value);
        }
    }
}
