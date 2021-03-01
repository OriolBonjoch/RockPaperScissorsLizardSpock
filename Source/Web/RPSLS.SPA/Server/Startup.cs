using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RPSLS.SPA.Server.Clients;
using RPSLS.SPA.Server.Config;
using RPSLS.SPA.Server.Services;
using System;

namespace RPSLS.SPA.Server
{
    public class Startup
    {
        private readonly string _apiVersion = "v1";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddControllersWithViews();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(_apiVersion, new OpenApiInfo
                {
                    Title = "Rock Paper Scissors Lizard Spock - Web BFF HTTP API",
                    Version = "v1"
                });
            });

            services.AddOptions();
            services.Configure<RecognitionSettings>(Configuration);
            services.Configure<GoogleAnalyticsSettings>(Configuration);
            services.Configure<TwitterOptions>(Configuration.GetSection("Authentication:Twitter"));
            services.Configure<GameManagerSettings>(Configuration.GetSection("GameManager"));
            services.ConfigureOptions<MultiplayerSettingsOptions>();
            if (Configuration.GetValue<bool>("GameManager:Grpc:GrpcOverHttp", false))
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2Support", true);
            }

            //services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
            services.AddScoped<IBotGameManagerClient, BotGameManagerClient>();
            services.AddScoped<IMultiplayerGameManagerClient, MultiplayerGameManagerClient>();
            services.AddSingleton<IConfigurationManagerClient, ConfigurationManagerClient>();
            services.AddScoped<IBotGameService, BotGameService>();
            services.AddScoped<IMultiplayerGameService, MultiplayerGameService>();
            //services.AddScoped<SvgHelper>();
            //services.AddScoped<BattleHelper>();

            services.AddSingleton(sp =>
            {
                return sp.GetService<IConfigurationManagerClient>().GetSettings();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{_apiVersion}/swagger.json", "Rock Paper Scissors Lizard Spock - Web BFF HTTP API");
            });


            //app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });

        }
    }
}
