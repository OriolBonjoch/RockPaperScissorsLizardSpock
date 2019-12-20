using Microsoft.Extensions.Configuration;
using RPSLS.Game.Multiplayer.Config;
using RPSLS.Game.Multiplayer.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MultiplayerExtensions
    {
        public static void AddMultiplayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MultiplayerSettings>(configuration.GetSection("Multiplayer"));
            services.AddTransient<ITokenService, TokenService>();
            services.AddSingleton<IPlayFabService, PlayFabService>();
        }
    } 
}
