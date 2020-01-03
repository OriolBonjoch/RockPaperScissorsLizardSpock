using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RPSLS.Web.Clients;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RPSLS.Web.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class TokenController : Controller
    {
        private const string BATTLE_URL = "/battle/multiplayer";
        private const string VALIDATE_URL = "/api/token/validate";
        private readonly ITokenManagerClient _tokenManager;

        public TokenController(
            ITokenManagerClient tokenManager)
        {
            _tokenManager = tokenManager;
        }

        [HttpGet("{token}")]
        //public IActionResult JoinGameAsync(string token)
        public async Task<IActionResult> JoinGameAsync(string token)
        {
            var redirect = $"{VALIDATE_URL}/{token}";

            //// TODO: remove code
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, "Paco")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(principal);
            return Redirect(redirect);
            //return Challenge(new AuthenticationProperties { RedirectUri = redirect }, "Twitter");
        }

        [HttpGet("validate/{token}")]
        public async Task<IActionResult> ValidateToken(string token)
        {
            var username = User.Identity.Name;
            await _tokenManager.Join(username, token);
            var matchFound = await _tokenManager.WaitMatch(username, (a, b) => { });
            return Redirect($"{BATTLE_URL}/{matchFound.MatchId}");
        }
    }
}