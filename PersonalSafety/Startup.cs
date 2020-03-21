using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PersonalSafety.Extensions;
using PersonalSafety.Models;
using PersonalSafety.Options;
using PersonalSafety.Business;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using System.Threading;
using SignalRChatServer;
using PersonalSafety.Installers;
using PersonalSafety.Hubs;
using SignalRChatServer.Hubs;
using Swashbuckle.AspNetCore.Filters;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Services;

namespace PersonalSafety
{
    public class Startup
    {
        readonly string ClientHubUrl = "/hubs/Client";
        readonly string AdminHubUrl = "/hubs/Admin";
        readonly string PersonnelHubUrl = "/hubs/Personnel";
        readonly string LocationTrackingHubUrl = "/hubs/Realtime";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                builder =>
                {
                    builder.AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                });
            });

            // Required for API functionality
            services.AddControllers();

            // Needed to display the home page "view"
            services.AddMvc().AddSessionStateTempDataProvider();
            services.AddSession();

            services.AddSignalR();

            // Setup the connextion string to be used by AppDbContext
            services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ConnectionString")));

            // Setup ASP Identity to use the database
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();



            // Registering basic JWT settings
            JwtSettings jwtSettings = new JwtSettings();
            Configuration.Bind(nameof(jwtSettings), jwtSettings);
            services.AddSingleton(jwtSettings);

            // Registering APP Settings
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // Registering FacebookAuth Settings
            var facebookAuthSettings = new FacebookAuthSettings();
            Configuration.Bind(nameof(FacebookAuthSettings), facebookAuthSettings);
            services.AddSingleton(facebookAuthSettings);

            // Add Services required by FacebookAuth
            services.AddHttpClient();

            // Add authentication middleware and set its parameters
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(auth => {
                    auth.SaveToken = true;
                    auth.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        RequireExpirationTime = false,
                        ValidateLifetime = true
                    };
                    auth.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && 
                                    (path.StartsWithSegments(ClientHubUrl) || path.StartsWithSegments(AdminHubUrl) || path.StartsWithSegments(PersonnelHubUrl)))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            // Register External Services Here
            services.AddSingleton<IFacebookAuthService, FacebookAuthService>();
            services.AddScoped<IJwtAuthService, JwtAuthService>();

            // Register Services
            services.AddScoped<IMainHub, MainHub>();
            services.AddScoped<IClientHub, ClientHub>();
            services.AddScoped<IPersonnelHub, PersonnelHub>();

            // Register Businesses
            services.AddScoped<IAccountBusiness, AccountBusiness>();
            services.AddScoped<IClientBusiness, ClientBusiness>();
            services.AddScoped<IAdminBusiness, AdminBusiness>();
            services.AddScoped<IPersonnelBusiness, PersonnelBusiness>();
            services.AddScoped<ISOSBusiness, SOSBusiness>();

            // Register here any Repositories that will be used:
            services.AddScoped<IEmergencyContactRepository, EmergencyContactRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IPersonnelRepository, PersonnelRepository>();
            services.AddScoped<ISOSRequestRepository, SOSRequestRepository>();

            // Setting up swagger generator
            services.AddSwaggerGen(sw => 
            {
                sw.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Personal Safety", Version = "V1" });
                sw.ExampleFilters();
                
                sw.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme 
                {
                    Description = "JWT Authorization header using bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                sw.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });

                // Activate swagger to sense XML comments
                string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                sw.IncludeXmlComments(xmlPath);

            });

            services.AddSwaggerExamplesFromAssemblyOf<LoginRequestViewModel>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, IClientRepository clientRepository, IPersonnelRepository personnelRepository)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // Allow headers required by SignalR, order is important
            app.Use((context, next) =>
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers.Where(h => h.Key == "Origin").FirstOrDefault().Value.ToString());
                context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                context.Response.Headers.Add("Access-Control-Allow-Methods", "*");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization, X-Requested-With");
                return next.Invoke();
            });

            app.UseCors();

            app.UseStaticFiles();

            serviceProvider.GetService<AppDbContext>().Database.EnsureCreated();
            
            ApplicationDbInitializer databaseInitializer = new ApplicationDbInitializer(userManager, clientRepository, personnelRepository);
            databaseInitializer.SeedUsers();

            app.UseSwagger();
            app.UseSwaggerUI(option => option.SwaggerEndpoint("/swagger/v1/swagger.json", "PersonalSafetyAPI Documentations"));

            //app.UseStatusCodePagesWithReExecute("/Error/{0}");

            //app.ConfigureExceptionHandler();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<RealtimeHub>(LocationTrackingHubUrl);
                endpoints.MapHub<ClientHub>(ClientHubUrl);
                endpoints.MapHub<AdminHub>(AdminHubUrl);
                endpoints.MapHub<PersonnelHub>(PersonnelHubUrl);
            });

            //app.UseMvc();
        }
    }
}
