﻿using RPSLS.SPA.Shared.Models;
using System;
using System.Threading.Tasks;

namespace RPSLS.SPA.Server.Services
{
    public interface IMultiplayerGameService : IGameService
    {
        string MatchId { get; set; }
        Task FetchMatchId(Action<string, string, string> matchIdCallback);
        Task FetchMatchId(string token);
        Task UserPick(int pick);
        Task AddGameListener(Action<ResultDto> gameListener);
        Task<bool> Rematch();
        Task<LeaderboardDto> GetLeaderboard();
    }
}
