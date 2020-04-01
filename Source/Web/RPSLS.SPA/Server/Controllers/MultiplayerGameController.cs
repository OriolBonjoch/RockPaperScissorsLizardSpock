using Microsoft.AspNetCore.Mvc;
using RPSLS.SPA.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPSLS.SPA.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MultiplayerGameController : ControllerBase
    {
        private readonly IMultiplayerGameService _multiplayerGameService;

        public MultiplayerGameController(IMultiplayerGameService multiplayerGameService)
        {
            _multiplayerGameService = multiplayerGameService;
        }
    }
}
