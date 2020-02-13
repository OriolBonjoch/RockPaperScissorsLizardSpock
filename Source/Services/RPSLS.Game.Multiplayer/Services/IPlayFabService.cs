﻿using RPSLS.Game.Multiplayer.Models;
using System.Threading.Tasks;

namespace RPSLS.Game.Multiplayer.Services
{
    public interface IPlayFabService
    {
        Task Initialize();
        Task<string> GetEntityToken(string token = null);
        Task<string> CreateTicket(string username, string token = "random");
        Task<MatchResult> CheckTicketStatus(string username, string ticketId);
        Task UpdateStats(string username, bool isWinner);
        Task<Leaderboard> GetLeaderboard();
    }
}
