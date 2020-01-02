using Grpc.Net.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RPSLS.Web.Config;
using System;
using System.Threading;
using System.Threading.Tasks;
using TokenApi.Proto;

namespace RPSLS.Web.Clients
{
    public class TokenManagerClient : BaseClient, ITokenManagerClient
    {
        private readonly string _serverUrl;

        public TokenManagerClient(IOptions<GameManagerSettings> settings, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _serverUrl = settings.Value.Url ?? throw new ArgumentNullException("Game Manager Url is null");
        }

        public async Task<string> CreateToken(string username)
        {
            var request = new CreateTokenRequest() { Username = username };
            var channel = GrpcChannel.ForAddress(_serverUrl);
            var client = new TokenManager.TokenManagerClient(channel);
            var result = await client.CreateTokenAsync(request, GetRequestMetadata());
            return result.Token;
        }

        public async Task Join(string username, string token)
        {
            var request = new JoinTokenRequest() { Username = username, Token = token };
            var channel = GrpcChannel.ForAddress(_serverUrl);
            var client = new TokenManager.TokenManagerClient(channel);
            await client.JoinAsync(request, GetRequestMetadata());
        }

        public async Task<string> WaitMatch(string username, Action<string, string> matchIdCallback)
        {
            var tokenSource = new CancellationTokenSource();
            var request = new MatchStatusRequest() { Username = username };
            var channel = GrpcChannel.ForAddress(_serverUrl);
            var client = new TokenManager.TokenManagerClient(channel);
            using var stream = client.WaitMatch(request, GetRequestMetadata());
            MatchStatusResponse response = null;
            while (await stream.ResponseStream.MoveNext(tokenSource.Token))
            {
                response = stream.ResponseStream.Current;
                matchIdCallback(response.MatchId, response.Status);
            }

            return response.MatchId;
        }
    }
}
