using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RPSLS.SPA.Shared.Config;

namespace RPSLS.SPA.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly MultiplayerSettings _multiplayerSettings;

        public SettingsController(IOptions<MultiplayerSettings> MultiplayerSettings)
        {
            _multiplayerSettings = MultiplayerSettings.Value;
        }

        public IActionResult Index()
        {
            return Ok(_multiplayerSettings);
        }
    }
}