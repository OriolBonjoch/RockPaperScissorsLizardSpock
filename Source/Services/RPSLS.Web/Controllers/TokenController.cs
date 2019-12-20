using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RPSLS.Web.Clients;
using System.Threading.Tasks;

namespace RPSLS.Web.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class TokenController : Controller
    {
        private const string redirectUri = "/battle";
        private const string redirectFromTwitter = "/api/token/validate";
        private readonly ITokenManagerClient _tokenManager;

        public TokenController(ITokenManagerClient tokenManager)
        {
            _tokenManager = tokenManager;
        }

        [HttpGet("{token}")]
        public IActionResult JoinGame(string token)
        {
            var redirect = $"{redirectFromTwitter}?token={token}&username={User.Identity.Name}";
            return Challenge(new AuthenticationProperties { RedirectUri = redirect }, "Twitter");
        }

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateToken(string token, string username)
        {
            const int DelayWait = 2000;
            const int MaxAttempts = 120000 / DelayWait;
            int count = 0;
            await _tokenManager.Join(username, token);
            while (count++ < MaxAttempts)
            {
                await Task.Delay(DelayWait);
                var hasMatch = await _tokenManager.Matched(User.Identity.Name);
                if (hasMatch)
                {
                    return Redirect(redirectUri);
                }
            }

            return Redirect("/login/twitter");
        }
    }
}