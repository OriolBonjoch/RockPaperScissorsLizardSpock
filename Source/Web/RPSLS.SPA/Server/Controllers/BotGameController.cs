using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RPSLS.SPA.Server.Services;
using RPSLS.SPA.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPSLS.SPA.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BotGameController : ControllerBase
    {
        private readonly IBotGameService _botGameService;
        public BotGameController(IBotGameService botGameService)
        {
            _botGameService = botGameService;
        }

        [HttpGet("play")]
        public async Task<StatusCodeResult> Play()
        {
            await _botGameService.Play();
            return Ok();
        }

        [HttpGet]
        [Route("challengers")]
        [ProducesResponseType(typeof(IEnumerable<ChallengerDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ChallengerDto>>> Challengers()
        {
            var botChallengers = await _botGameService.Challengers();
            return Ok(botChallengers);
        }

        [HttpPut]
        [Route("challenger")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public StatusCodeResult SetChallenger(ChallengerDto challenger)
        {
            _botGameService.SetChallenger(challenger);
            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
