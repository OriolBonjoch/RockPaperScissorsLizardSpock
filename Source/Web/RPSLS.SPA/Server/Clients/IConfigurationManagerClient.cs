using RPSLS.SPA.Shared.Models;

namespace RPSLS.SPA.Server.Clients
{
    public interface IConfigurationManagerClient
    {
        GameSettingsDto GetSettings();
    }
}
