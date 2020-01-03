﻿using GameApi.Proto;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RPSLS.Web.Config;
using RPSLS.Web.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLS.Web.Clients
{
    public class MultiplayerGameManagerClient : BaseClient, IMultiplayerGameManagerClient
    {
        private readonly string _serverUrl;

        public MultiplayerGameManagerClient(IOptions<GameManagerSettings> settings, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _serverUrl = settings.Value.Url ?? throw new ArgumentNullException("Game Manager Url is null");
        }

        public async Task<string> CreatePairing(string username)
        {
            var request = new CreatePairingRequest() { Username = username };
            var channel = GrpcChannel.ForAddress(_serverUrl);
            var client = new MultiplayerGameManager.MultiplayerGameManagerClient(channel);
            var result = await client.CreatePairingAsync(request, GetRequestMetadata());
            return result.Token;
        }

        public async Task JoinPairing(string username, string token)
        {
            var request = new JoinPairingRequest() { Username = username, Token = token };
            var channel = GrpcChannel.ForAddress(_serverUrl);
            var client = new MultiplayerGameManager.MultiplayerGameManagerClient(channel);
            await client.JoinPairingAsync(request, GetRequestMetadata());
        }

        public async Task<MatchFoundDto> PairingStatus(string username, Action<string, string> matchIdCallback)
        {
            var tokenSource = new CancellationTokenSource();
            var request = new PairingStatusRequest() { Username = username };
            var channel = GrpcChannel.ForAddress(_serverUrl);
            var client = new MultiplayerGameManager.MultiplayerGameManagerClient(channel);
            using var stream = client.PairingStatus(request, GetRequestMetadata());
            PairingStatusResponse response = null;
            while (await stream.ResponseStream.MoveNext(tokenSource.Token))
            {
                response = stream.ResponseStream.Current;
                matchIdCallback(response.MatchId, response.Status);
            }

            return new MatchFoundDto { MatchId = response.MatchId };
        }
    }
}
